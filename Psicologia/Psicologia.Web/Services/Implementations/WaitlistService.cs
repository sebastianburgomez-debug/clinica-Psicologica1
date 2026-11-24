using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.Appointment;
using Psicologia.Web.DTOs.Waitlist;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Lista de espera: la secretaria registra pacientes sin cupo y les asigna una cita cuando se libera uno
    public class WaitlistService : CustomQueryableOperationsService, IWaitlistService
    {
        private readonly DataContext _context;
        private readonly IAccountService _accountService;
        private readonly IAppointmentsService _appointmentsService;
        private readonly IAuditService _auditService;

        public WaitlistService(DataContext context,
                               IMapper mapper,
                               IAccountService accountService,
                               IAppointmentsService appointmentsService,
                               IAuditService auditService) : base(context, mapper)
        {
            _context = context;
            _accountService = accountService;
            _appointmentsService = appointmentsService;
            _auditService = auditService;
        }

        public async Task<Response<PaginationResponse<WaitlistEntryDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<WaitlistEntry> query = _context.WaitlistEntries.AsNoTracking()
                                                                      .Include(w => w.Patient)
                                                                      .Include(w => w.ClinicalService)
                                                                      .Include(w => w.DoctorProfile)
                                                                          .ThenInclude(d => d!.User);

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();
                query = query.Where(w => w.Patient!.Name.ToLower().Contains(filter)
                                         || w.ClinicalService!.Name.ToLower().Contains(filter));
            }

            // Primero los que siguen en espera y, entre ellos, el más antiguo (turno de llegada)
            query = query.OrderBy(w => w.Status).ThenBy(w => w.CreatedAt);

            return await GetPagedListAsync<WaitlistEntryDTO, WaitlistEntry>(request, query);
        }

        public async Task<Response> CreateAsync(CreateWaitlistEntryDTO dto)
        {
            try
            {
                if (dto.PatientId is null || dto.ClinicalServiceId is null)
                {
                    return Response.Failure("El paciente y el servicio son requeridos");
                }

                User? patient = await _context.Users.Include(u => u.Role)
                                                    .FirstOrDefaultAsync(u => u.Id == dto.PatientId);
                if (patient is null || !patient.IsActive || patient.Role?.Name != Env.PATIENT_ROLE_NAME)
                {
                    return Response.Failure("El paciente seleccionado no existe o está inactivo");
                }

                bool serviceExists = await _context.ClinicalServices.AnyAsync(cs => cs.Id == dto.ClinicalServiceId && cs.IsActive);
                if (!serviceExists)
                {
                    return Response.Failure("El servicio seleccionado no existe o está inactivo");
                }

                if (dto.DoctorProfileId is not null)
                {
                    bool doctorExists = await _context.DoctorProfiles.AnyAsync(d => d.Id == dto.DoctorProfileId
                                                                                    && d.Status == ApprovalStatus.Approved
                                                                                    && d.User!.IsActive);
                    if (!doctorExists)
                    {
                        return Response.Failure("El doctor seleccionado no está aprobado o su cuenta está inactiva");
                    }
                }

                if (dto.PreferredDate is not null && dto.PreferredDate.Value < DateOnly.FromDateTime(DateTime.Today))
                {
                    return Response.Failure("La fecha preferida no puede estar en el pasado");
                }

                // Evita duplicados: el mismo paciente esperando el mismo servicio
                bool alreadyWaiting = await _context.WaitlistEntries.AnyAsync(w => w.PatientId == patient.Id
                                                                                   && w.ClinicalServiceId == dto.ClinicalServiceId
                                                                                   && w.Status == WaitlistStatus.Waiting);
                if (alreadyWaiting)
                {
                    return Response.Failure("El paciente ya está en la lista de espera para ese servicio");
                }

                User? accountUser = await _accountService.GetAccountUserAsync();

                WaitlistEntry entry = new WaitlistEntry
                {
                    Id = Guid.NewGuid(),
                    PatientId = patient.Id,
                    ClinicalServiceId = dto.ClinicalServiceId.Value,
                    DoctorProfileId = dto.DoctorProfileId,
                    PreferredDate = dto.PreferredDate,
                    Notes = dto.Notes?.Trim(),
                    Status = WaitlistStatus.Waiting,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = accountUser?.Email
                };

                await _context.WaitlistEntries.AddAsync(entry);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.CREATE, "Lista de espera", entry.Id.ToString(),
                                             $"Paciente {patient.Name} agregado a la lista de espera");

                return Response.Success("Paciente agregado a la lista de espera");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        public async Task<Response<AssignWaitlistEntryDTO>> GetForAssignAsync(Guid id)
        {
            try
            {
                WaitlistEntry? entry = await _context.WaitlistEntries.AsNoTracking()
                                                                     .Include(w => w.Patient)
                                                                     .Include(w => w.ClinicalService)
                                                                     .FirstOrDefaultAsync(w => w.Id == id);
                if (entry is null)
                {
                    return Response<AssignWaitlistEntryDTO>.Failure("No existe el registro de la lista de espera");
                }

                if (entry.Status != WaitlistStatus.Waiting)
                {
                    return Response<AssignWaitlistEntryDTO>.Failure("Solo se pueden asignar los registros que siguen en espera");
                }

                AssignWaitlistEntryDTO dto = new AssignWaitlistEntryDTO
                {
                    Id = entry.Id,
                    DoctorProfileId = entry.DoctorProfileId,
                    Date = entry.PreferredDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    StartTime = new TimeOnly(8, 0),
                    PatientName = entry.Patient?.Name,
                    ServiceName = entry.ClinicalService?.Name
                };

                return Response<AssignWaitlistEntryDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<AssignWaitlistEntryDTO>.Failure(ex);
            }
        }

        public async Task<Response> AssignAsync(AssignWaitlistEntryDTO dto)
        {
            try
            {
                WaitlistEntry? entry = await _context.WaitlistEntries.FirstOrDefaultAsync(w => w.Id == dto.Id);
                if (entry is null)
                {
                    return Response.Failure("No existe el registro de la lista de espera");
                }

                if (entry.Status != WaitlistStatus.Waiting)
                {
                    return Response.Failure("Solo se pueden asignar los registros que siguen en espera");
                }

                // La creación de la cita ya valida disponibilidad, cruces, doctor aprobado y servicio
                Response<AppointmentDTO> created = await _appointmentsService.CreateAsync(new CreateAppointmentDTO
                {
                    DoctorProfileId = dto.DoctorProfileId,
                    PatientId = entry.PatientId,
                    ClinicalServiceId = entry.ClinicalServiceId,
                    Date = dto.Date,
                    StartTime = dto.StartTime,
                    Notes = entry.Notes
                });

                if (!created.IsSuccess || created.Result is null)
                {
                    return Response.Failure(created.Errors?.FirstOrDefault() ?? created.Message ?? "No fue posible agendar la cita");
                }

                entry.Status = WaitlistStatus.Assigned;
                entry.AssignedAppointmentId = created.Result.Id;
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Lista de espera", entry.Id.ToString(),
                                             $"Cupo asignado desde la lista de espera para la cita del {created.Result.StartsAt:dd/MM/yyyy HH:mm}");

                return Response.Success("Cupo asignado y cita agendada con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        public async Task<Response> CancelAsync(Guid id)
        {
            try
            {
                WaitlistEntry? entry = await _context.WaitlistEntries.FirstOrDefaultAsync(w => w.Id == id);
                if (entry is null)
                {
                    return Response.Failure("No existe el registro de la lista de espera");
                }

                if (entry.Status != WaitlistStatus.Waiting)
                {
                    return Response.Failure("Solo se pueden cancelar los registros que siguen en espera");
                }

                entry.Status = WaitlistStatus.Cancelled;
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Lista de espera", entry.Id.ToString(),
                                             "Registro de la lista de espera cancelado");

                return Response.Success("Registro cancelado");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }
    }
}

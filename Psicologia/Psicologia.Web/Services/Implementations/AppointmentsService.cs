using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Extensions;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.Appointment;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Citas: base de los indicadores de ocupación (sin videollamadas, recordatorios ni facturación)
    public class AppointmentsService : CustomQueryableOperationsService, IAppointmentsService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly IAuditService _auditService;

        public AppointmentsService(DataContext context,
                                   IMapper mapper,
                                   IAccountService accountService,
                                   IAuditService auditService) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _accountService = accountService;
            _auditService = auditService;
        }

        public async Task<Response<PaginationResponse<AppointmentDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<Appointment> query = _context.Appointments.AsNoTracking()
                                                                 .Include(a => a.Patient)
                                                                 .Include(a => a.ClinicalService)
                                                                 .Include(a => a.DoctorProfile)
                                                                     .ThenInclude(d => d!.User);

            // Un doctor solo ve sus propias citas
            DoctorProfile? ownProfile = await _accountService.GetAccountDoctorProfileAsync();

            if (ownProfile is not null)
            {
                query = query.Where(a => a.DoctorProfileId == ownProfile.Id);
            }

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                query = query.Where(a => a.Patient!.Name.ToLower().Contains(filter)
                                         || a.DoctorProfile!.User!.Name.ToLower().Contains(filter)
                                         || a.ClinicalService!.Name.ToLower().Contains(filter));
            }

            query = query.OrderByDescending(a => a.StartsAt);

            return await GetPagedListAsync<AppointmentDTO, Appointment>(request, query);
        }

        public async Task<Response<AppointmentDTO>> CreateAsync(CreateAppointmentDTO dto)
        {
            try
            {
                if (dto.DoctorProfileId is null || dto.PatientId is null || dto.ClinicalServiceId is null
                    || dto.Date is null || dto.StartTime is null)
                {
                    return Response<AppointmentDTO>.Failure("Todos los campos de la cita son requeridos");
                }

                DoctorProfile? ownProfile = await _accountService.GetAccountDoctorProfileAsync();

                if (ownProfile is not null && ownProfile.Id != dto.DoctorProfileId)
                {
                    return Response<AppointmentDTO>.Failure("Solo puede agendar citas en su propia agenda");
                }

                DoctorProfile? doctor = await _context.DoctorProfiles.Include(d => d.User)
                                                                     .FirstOrDefaultAsync(d => d.Id == dto.DoctorProfileId);

                if (doctor is null || doctor.Status != ApprovalStatus.Approved || doctor.User is null || !doctor.User.IsActive)
                {
                    return Response<AppointmentDTO>.Failure("El doctor seleccionado no está aprobado o su cuenta está inactiva");
                }

                User? patient = await _context.Users.Include(u => u.Role)
                                                    .FirstOrDefaultAsync(u => u.Id == dto.PatientId);

                if (patient is null || !patient.IsActive || patient.Role?.Name != Env.PATIENT_ROLE_NAME)
                {
                    return Response<AppointmentDTO>.Failure("El paciente seleccionado no existe o está inactivo");
                }

                ClinicalService? service = await _context.ClinicalServices.FirstOrDefaultAsync(cs => cs.Id == dto.ClinicalServiceId && cs.IsActive);

                if (service is null)
                {
                    return Response<AppointmentDTO>.Failure("El servicio seleccionado no existe o está inactivo");
                }

                if (service.SpecialtyId is not null && service.SpecialtyId != doctor.SpecialtyId)
                {
                    return Response<AppointmentDTO>.Failure("El servicio seleccionado pertenece a una especialidad distinta a la del doctor");
                }

                DateTime startsAt = dto.Date.Value.ToDateTime(dto.StartTime.Value);
                DateTime endsAt = startsAt.AddMinutes(service.DurationMinutes);

                if (startsAt <= DateTime.Now)
                {
                    return Response<AppointmentDTO>.Failure("La cita debe programarse en una fecha y hora futura");
                }

                if (endsAt.Date != startsAt.Date)
                {
                    return Response<AppointmentDTO>.Failure("La cita debe iniciar y terminar el mismo día");
                }

                // Debe caber dentro de un bloque de disponibilidad del doctor
                TimeOnly startTime = TimeOnly.FromDateTime(startsAt);
                TimeOnly endTime = TimeOnly.FromDateTime(endsAt);

                bool fitsAvailability = await _context.DoctorAvailabilities.AnyAsync(a => a.DoctorProfileId == doctor.Id
                                                                                          && a.DayOfWeek == startsAt.DayOfWeek
                                                                                          && a.StartTime <= startTime
                                                                                          && a.EndTime >= endTime);
                if (!fitsAvailability)
                {
                    return Response<AppointmentDTO>.Failure($"El horario no está dentro de la disponibilidad del doctor para el {startsAt.DayOfWeek.ToSpanish().ToLower()}");
                }

                bool doctorBusy = await _context.Appointments.AnyAsync(a => a.DoctorProfileId == doctor.Id
                                                                            && a.Status != AppointmentStatus.Cancelled
                                                                            && a.StartsAt < endsAt
                                                                            && startsAt < a.EndsAt);
                if (doctorBusy)
                {
                    return Response<AppointmentDTO>.Failure("El doctor ya tiene una cita en ese horario");
                }

                bool patientBusy = await _context.Appointments.AnyAsync(a => a.PatientId == patient.Id
                                                                             && a.Status != AppointmentStatus.Cancelled
                                                                             && a.StartsAt < endsAt
                                                                             && startsAt < a.EndsAt);
                if (patientBusy)
                {
                    return Response<AppointmentDTO>.Failure("El paciente ya tiene una cita en ese horario");
                }

                User? accountUser = await _accountService.GetAccountUserAsync();

                Appointment appointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    DoctorProfileId = doctor.Id,
                    PatientId = patient.Id,
                    ClinicalServiceId = service.Id,
                    StartsAt = startsAt,
                    EndsAt = endsAt,
                    Status = AppointmentStatus.Scheduled,
                    Notes = dto.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = accountUser?.Email
                };

                await _context.Appointments.AddAsync(appointment);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.CREATE, "Cita", appointment.Id.ToString(),
                                             $"Cita agendada el {startsAt:dd/MM/yyyy HH:mm} con {doctor.User.Name} para {patient.Name}");

                AppointmentDTO result = new AppointmentDTO
                {
                    Id = appointment.Id,
                    DoctorProfileId = doctor.Id,
                    DoctorName = doctor.User.Name,
                    PatientName = patient.Name,
                    ServiceName = service.Name,
                    StartsAt = startsAt,
                    EndsAt = endsAt,
                    Status = appointment.Status,
                    Notes = appointment.Notes
                };

                return Response<AppointmentDTO>.Success(result, "Cita agendada con éxito");
            }
            catch (Exception ex)
            {
                return Response<AppointmentDTO>.Failure(ex);
            }
        }

        public async Task<Response> ChangeStatusAsync(ChangeAppointmentStatusDTO dto)
        {
            try
            {
                Appointment? appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == dto.Id);

                if (appointment is null)
                {
                    return Response.Failure($"No existe cita con id: {dto.Id}");
                }

                DoctorProfile? ownProfile = await _accountService.GetAccountDoctorProfileAsync();

                if (ownProfile is not null && ownProfile.Id != appointment.DoctorProfileId)
                {
                    return Response.Failure("Solo puede gestionar sus propias citas");
                }

                if (appointment.Status != AppointmentStatus.Scheduled)
                {
                    return Response.Failure("Solo se puede cambiar el estado de las citas programadas");
                }

                if (dto.Status == AppointmentStatus.Scheduled)
                {
                    return Response.Failure("Seleccione un estado distinto a 'Programada'");
                }

                if ((dto.Status == AppointmentStatus.Attended || dto.Status == AppointmentStatus.NoShow) && appointment.StartsAt > DateTime.Now)
                {
                    return Response.Failure("No se puede marcar como atendida o inasistida una cita que aún no ha iniciado");
                }

                appointment.Status = dto.Status;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Cita", appointment.Id.ToString(),
                                             $"Cita del {appointment.StartsAt:dd/MM/yyyy HH:mm} marcada como '{dto.Status.GetDisplayName()}'");

                return Response.Success($"Cita marcada como '{dto.Status.GetDisplayName()}'");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }
    }
}

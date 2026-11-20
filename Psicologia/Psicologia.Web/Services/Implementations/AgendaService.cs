using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Extensions;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.Agenda;
using Psicologia.Web.DTOs.Appointment;
using Psicologia.Web.DTOs.Policy;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Agenda personal: disponibilidad horaria semanal de cada doctor
    public class AgendaService : IAgendaService
    {
        private const int MIN_BLOCK_MINUTES = 15;

        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly IPoliciesService _policiesService;
        private readonly IAuditService _auditService;

        public AgendaService(DataContext context,
                             IMapper mapper,
                             IAccountService accountService,
                             IPoliciesService policiesService,
                             IAuditService auditService)
        {
            _context = context;
            _mapper = mapper;
            _accountService = accountService;
            _policiesService = policiesService;
            _auditService = auditService;
        }

        public async Task<Response<AgendaDTO>> GetAgendaAsync(Guid? doctorProfileId, DateTime? weekStart)
        {
            try
            {
                DoctorProfile? ownProfile = await _accountService.GetAccountDoctorProfileAsync();

                DateTime start = (weekStart ?? DateTime.Today).StartOfWeek();

                AgendaDTO dto = new AgendaDTO
                {
                    CanSelectDoctor = ownProfile is null,
                    WeekStart = start,
                    BusinessHours = await _policiesService.GetBusinessHoursAsync()
                };

                // Un doctor solo ve su propia agenda; administración/secretaría puede elegir el doctor
                Guid? targetId = ownProfile?.Id ?? doctorProfileId;

                if (targetId is null)
                {
                    targetId = await _context.DoctorProfiles.OrderBy(d => d.User!.Name)
                                                            .Select(d => (Guid?)d.Id)
                                                            .FirstOrDefaultAsync();
                }

                if (targetId is null)
                {
                    // Aún no hay doctores registrados
                    return Response<AgendaDTO>.Success(dto);
                }

                DoctorProfile? profile = await _context.DoctorProfiles.AsNoTracking()
                                                                      .Include(d => d.User)
                                                                      .Include(d => d.Specialty)
                                                                      .FirstOrDefaultAsync(d => d.Id == targetId);
                if (profile is null)
                {
                    return Response<AgendaDTO>.Failure("No existe el doctor seleccionado");
                }

                List<DoctorAvailability> availabilities = await _context.DoctorAvailabilities.AsNoTracking()
                                                                                             .Where(a => a.DoctorProfileId == profile.Id)
                                                                                             .ToListAsync();

                DateTime end = start.AddDays(7);

                List<Appointment> appointments = await _context.Appointments.AsNoTracking()
                                                                            .Include(a => a.Patient)
                                                                            .Include(a => a.ClinicalService)
                                                                            .Include(a => a.DoctorProfile)
                                                                                .ThenInclude(d => d!.User)
                                                                            .Where(a => a.DoctorProfileId == profile.Id
                                                                                        && a.StartsAt >= start
                                                                                        && a.StartsAt < end)
                                                                            .OrderBy(a => a.StartsAt)
                                                                            .ToListAsync();

                dto.DoctorProfileId = profile.Id;
                dto.DoctorName = profile.User?.Name;
                dto.SpecialtyName = profile.Specialty?.Name;
                dto.DoctorStatus = profile.Status;
                dto.Availabilities = availabilities.OrderBy(a => Array.IndexOf(DateExtensions.WeekDays, a.DayOfWeek))
                                                   .ThenBy(a => a.StartTime)
                                                   .Select(a => _mapper.Map<AvailabilityDTO>(a))
                                                   .ToList();
                dto.WeeklyAvailableMinutes = OccupancyHelper.WeeklyMinutes(availabilities);
                dto.WeekAppointments = _mapper.Map<List<AppointmentDTO>>(appointments);
                dto.NewAvailability = new CreateAvailabilityDTO { DoctorProfileId = profile.Id };

                return Response<AgendaDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<AgendaDTO>.Failure(ex);
            }
        }

        public async Task<Response> CreateAvailabilityAsync(CreateAvailabilityDTO dto)
        {
            try
            {
                Response ownership = await CheckOwnershipAsync(dto.DoctorProfileId);

                if (!ownership.IsSuccess)
                {
                    return ownership;
                }

                if (dto.StartTime >= dto.EndTime)
                {
                    return Response.Failure("La hora de inicio debe ser menor que la hora de fin");
                }

                if (DateExtensions.TotalMinutes(dto.StartTime, dto.EndTime) < MIN_BLOCK_MINUTES)
                {
                    return Response.Failure($"El bloque debe durar al menos {MIN_BLOCK_MINUTES} minutos");
                }

                // El bloque debe estar dentro del horario de atención de la clínica (Políticas)
                List<BusinessHourDTO> businessHours = await _policiesService.GetBusinessHoursAsync();
                BusinessHourDTO businessHour = businessHours.First(b => b.DayOfWeek == dto.DayOfWeek);

                if (!businessHour.IsOpen)
                {
                    return Response.Failure($"La clínica no atiende los días {dto.DayOfWeek.ToSpanish().ToLower()}");
                }

                if (dto.StartTime < businessHour.OpenTime || dto.EndTime > businessHour.CloseTime)
                {
                    return Response.Failure($"El bloque debe estar dentro del horario de atención del {dto.DayOfWeek.ToSpanish().ToLower()} " +
                                            $"({businessHour.OpenTime:HH\\:mm} - {businessHour.CloseTime:HH\\:mm})");
                }

                bool overlaps = await _context.DoctorAvailabilities.AnyAsync(a => a.DoctorProfileId == dto.DoctorProfileId
                                                                                  && a.DayOfWeek == dto.DayOfWeek
                                                                                  && a.StartTime < dto.EndTime
                                                                                  && dto.StartTime < a.EndTime);
                if (overlaps)
                {
                    return Response.Failure("El bloque se cruza con otro bloque de disponibilidad del mismo día");
                }

                DoctorAvailability availability = new DoctorAvailability
                {
                    Id = Guid.NewGuid(),
                    DoctorProfileId = dto.DoctorProfileId,
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime
                };

                await _context.DoctorAvailabilities.AddAsync(availability);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.CREATE, "Agenda", availability.Id.ToString(),
                                             $"Disponibilidad agregada: {dto.DayOfWeek.ToSpanish()} {dto.StartTime:HH\\:mm} - {dto.EndTime:HH\\:mm}");

                return Response.Success("Disponibilidad agregada con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        public async Task<Response<Guid>> DeleteAvailabilityAsync(Guid id)
        {
            try
            {
                DoctorAvailability? availability = await _context.DoctorAvailabilities.FirstOrDefaultAsync(a => a.Id == id);

                if (availability is null)
                {
                    return Response<Guid>.Failure($"No existe el bloque de disponibilidad con id: {id}");
                }

                Response ownership = await CheckOwnershipAsync(availability.DoctorProfileId);

                if (!ownership.IsSuccess)
                {
                    return Response<Guid>.Failure(ownership.Message ?? "No autorizado");
                }

                _context.DoctorAvailabilities.Remove(availability);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.DELETE, "Agenda", id.ToString(),
                                             $"Disponibilidad eliminada: {availability.DayOfWeek.ToSpanish()} {availability.StartTime:HH\\:mm} - {availability.EndTime:HH\\:mm}");

                return Response<Guid>.Success(availability.DoctorProfileId, "Disponibilidad eliminada con éxito");
            }
            catch (Exception ex)
            {
                return Response<Guid>.Failure(ex);
            }
        }

        // Un doctor solo gestiona su propia agenda
        private async Task<Response> CheckOwnershipAsync(Guid doctorProfileId)
        {
            bool exists = await _context.DoctorProfiles.AnyAsync(d => d.Id == doctorProfileId);

            if (!exists)
            {
                return Response.Failure("No existe el doctor seleccionado");
            }

            DoctorProfile? ownProfile = await _accountService.GetAccountDoctorProfileAsync();

            if (ownProfile is not null && ownProfile.Id != doctorProfileId)
            {
                return Response.Failure("Solo puede gestionar su propia agenda");
            }

            return Response.Success();
        }
    }
}

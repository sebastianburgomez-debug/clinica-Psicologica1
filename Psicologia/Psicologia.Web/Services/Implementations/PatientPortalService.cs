using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Extensions;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.Appointment;
using Psicologia.Web.DTOs.Portal;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Portal del paciente: solicitar, reprogramar y cancelar citas propias, y consultar su historial
    public class PatientPortalService : IPatientPortalService
    {
        // Anticipación mínima para cancelar o reprogramar una cita
        private const int MIN_HOURS_TO_CHANGE = 2;

        // Cada cuántos minutos se ofrece un inicio de cita dentro de un bloque de disponibilidad
        private const int SLOT_STEP_MINUTES = 30;

        private readonly DataContext _context;
        private readonly IAccountService _accountService;
        private readonly IAppointmentsService _appointmentsService;
        private readonly IAuditService _auditService;

        public PatientPortalService(DataContext context,
                                    IAccountService accountService,
                                    IAppointmentsService appointmentsService,
                                    IAuditService auditService)
        {
            _context = context;
            _accountService = accountService;
            _appointmentsService = appointmentsService;
            _auditService = auditService;
        }

        // Usuario de la sesión, solo si es paciente (null en cualquier otro caso)
        private async Task<User?> GetPatientAsync()
        {
            User? user = await _accountService.GetAccountUserAsync();
            return user?.Role?.Name == Env.PATIENT_ROLE_NAME ? user : null;
        }

        public async Task<Response<List<PatientAppointmentDTO>>> GetMyAppointmentsAsync()
        {
            try
            {
                User? patient = await GetPatientAsync();
                if (patient is null)
                {
                    return Response<List<PatientAppointmentDTO>>.Failure("El portal está disponible solo para pacientes");
                }

                List<PatientAppointmentDTO> items = await _context.Appointments.AsNoTracking()
                                                                               .Where(a => a.PatientId == patient.Id)
                                                                               .OrderByDescending(a => a.StartsAt)
                                                                               .Select(a => new PatientAppointmentDTO
                                                                               {
                                                                                   Id = a.Id,
                                                                                   DoctorName = a.DoctorProfile!.User!.Name,
                                                                                   ServiceName = a.ClinicalService!.Name,
                                                                                   StartsAt = a.StartsAt,
                                                                                   EndsAt = a.EndsAt,
                                                                                   Status = a.Status,
                                                                                   Notes = a.Notes,
                                                                                   HasReview = _context.Reviews.Any(r => r.AppointmentId == a.Id)
                                                                               }).ToListAsync();

                DateTime limit = DateTime.Now.AddHours(MIN_HOURS_TO_CHANGE);
                foreach (PatientAppointmentDTO item in items)
                {
                    bool canChange = item.Status == AppointmentStatus.Scheduled && item.StartsAt > limit;
                    item.CanCancel = canChange;
                    item.CanReschedule = canChange;
                    item.CanReview = item.Status == AppointmentStatus.Attended && !item.HasReview;
                }

                return Response<List<PatientAppointmentDTO>>.Success(items);
            }
            catch (Exception ex)
            {
                return Response<List<PatientAppointmentDTO>>.Failure(ex);
            }
        }

        public async Task<Response<AppointmentDTO>> BookAsync(BookAppointmentDTO dto)
        {
            User? patient = await GetPatientAsync();
            if (patient is null)
            {
                return Response<AppointmentDTO>.Failure("Solo los pacientes pueden solicitar citas desde el portal");
            }

            // Se reutilizan todas las validaciones de la creación de citas (disponibilidad, cruces, servicio, etc.)
            // pero el paciente SIEMPRE es el de la sesión, nunca uno enviado desde el formulario
            return await _appointmentsService.CreateAsync(new CreateAppointmentDTO
            {
                DoctorProfileId = dto.DoctorProfileId,
                PatientId = patient.Id,
                ClinicalServiceId = dto.ClinicalServiceId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                Notes = dto.Notes
            });
        }

        public async Task<Response> CancelAsync(Guid appointmentId)
        {
            try
            {
                User? patient = await GetPatientAsync();
                if (patient is null)
                {
                    return Response.Failure("Solo los pacientes pueden cancelar citas desde el portal");
                }

                // Filtrar por PatientId impide cancelar citas de otro paciente
                Appointment? appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patient.Id);
                if (appointment is null)
                {
                    return Response.Failure("No existe la cita indicada");
                }

                if (appointment.Status != AppointmentStatus.Scheduled)
                {
                    return Response.Failure("Solo se pueden cancelar las citas programadas");
                }

                if (appointment.StartsAt <= DateTime.Now.AddHours(MIN_HOURS_TO_CHANGE))
                {
                    return Response.Failure($"Solo puede cancelar con al menos {MIN_HOURS_TO_CHANGE} horas de anticipación");
                }

                appointment.Status = AppointmentStatus.Cancelled;
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Cita", appointment.Id.ToString(),
                                             $"El paciente {patient.Name} canceló su cita del {appointment.StartsAt:dd/MM/yyyy HH:mm}");

                return Response.Success("Cita cancelada con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        public async Task<Response<RescheduleAppointmentDTO>> GetForRescheduleAsync(Guid appointmentId)
        {
            try
            {
                User? patient = await GetPatientAsync();
                if (patient is null)
                {
                    return Response<RescheduleAppointmentDTO>.Failure("Solo los pacientes pueden reprogramar citas desde el portal");
                }

                Appointment? appointment = await _context.Appointments.AsNoTracking()
                                                                      .Include(a => a.ClinicalService)
                                                                      .Include(a => a.DoctorProfile)
                                                                          .ThenInclude(d => d!.User)
                                                                      .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patient.Id);
                if (appointment is null)
                {
                    return Response<RescheduleAppointmentDTO>.Failure("No existe la cita indicada");
                }

                if (appointment.Status != AppointmentStatus.Scheduled || appointment.StartsAt <= DateTime.Now.AddHours(MIN_HOURS_TO_CHANGE))
                {
                    return Response<RescheduleAppointmentDTO>.Failure($"Solo puede reprogramar citas programadas con al menos {MIN_HOURS_TO_CHANGE} horas de anticipación");
                }

                RescheduleAppointmentDTO dto = new RescheduleAppointmentDTO
                {
                    Id = appointment.Id,
                    DoctorProfileId = appointment.DoctorProfileId,
                    ClinicalServiceId = appointment.ClinicalServiceId,
                    DoctorName = appointment.DoctorProfile?.User?.Name,
                    ServiceName = appointment.ClinicalService?.Name,
                    CurrentStartsAt = appointment.StartsAt,
                    Date = DateOnly.FromDateTime(appointment.StartsAt)
                };

                return Response<RescheduleAppointmentDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<RescheduleAppointmentDTO>.Failure(ex);
            }
        }

        public async Task<Response> RescheduleAsync(RescheduleAppointmentDTO dto)
        {
            try
            {
                User? patient = await GetPatientAsync();
                if (patient is null)
                {
                    return Response.Failure("Solo los pacientes pueden reprogramar citas desde el portal");
                }

                if (dto.Date is null || dto.StartTime is null)
                {
                    return Response.Failure("La nueva fecha y hora son requeridas");
                }

                Appointment? appointment = await _context.Appointments.Include(a => a.ClinicalService)
                                                                      .FirstOrDefaultAsync(a => a.Id == dto.Id && a.PatientId == patient.Id);
                if (appointment is null || appointment.ClinicalService is null)
                {
                    return Response.Failure("No existe la cita indicada");
                }

                DateTime limit = DateTime.Now.AddHours(MIN_HOURS_TO_CHANGE);
                if (appointment.Status != AppointmentStatus.Scheduled || appointment.StartsAt <= limit)
                {
                    return Response.Failure($"Solo puede reprogramar citas programadas con al menos {MIN_HOURS_TO_CHANGE} horas de anticipación");
                }

                // El doctor y el servicio salen de la cita guardada, nunca del formulario
                DateTime startsAt = dto.Date.Value.ToDateTime(dto.StartTime.Value);
                DateTime endsAt = startsAt.AddMinutes(appointment.ClinicalService.DurationMinutes);

                if (startsAt <= limit)
                {
                    return Response.Failure($"La nueva cita debe tener al menos {MIN_HOURS_TO_CHANGE} horas de anticipación");
                }

                if (endsAt.Date != startsAt.Date)
                {
                    return Response.Failure("La cita debe iniciar y terminar el mismo día");
                }

                TimeOnly startTime = TimeOnly.FromDateTime(startsAt);
                TimeOnly endTime = TimeOnly.FromDateTime(endsAt);
                bool fitsAvailability = await _context.DoctorAvailabilities.AnyAsync(a => a.DoctorProfileId == appointment.DoctorProfileId
                                                                                          && a.DayOfWeek == startsAt.DayOfWeek
                                                                                          && a.StartTime <= startTime
                                                                                          && a.EndTime >= endTime);
                if (!fitsAvailability)
                {
                    return Response.Failure($"El horario no está dentro de la disponibilidad del doctor para el {startsAt.DayOfWeek.ToSpanish().ToLower()}");
                }

                // Los cruces ignoran la propia cita (a.Id != appointment.Id) para poder moverla en el mismo día
                bool doctorBusy = await _context.Appointments.AnyAsync(a => a.Id != appointment.Id
                                                                            && a.DoctorProfileId == appointment.DoctorProfileId
                                                                            && a.Status != AppointmentStatus.Cancelled
                                                                            && a.StartsAt < endsAt
                                                                            && startsAt < a.EndsAt);
                if (doctorBusy)
                {
                    return Response.Failure("El doctor ya tiene una cita en ese horario");
                }

                bool patientBusy = await _context.Appointments.AnyAsync(a => a.Id != appointment.Id
                                                                             && a.PatientId == patient.Id
                                                                             && a.Status != AppointmentStatus.Cancelled
                                                                             && a.StartsAt < endsAt
                                                                             && startsAt < a.EndsAt);
                if (patientBusy)
                {
                    return Response.Failure("Ya tiene una cita en ese horario");
                }

                DateTime previous = appointment.StartsAt;
                appointment.StartsAt = startsAt;
                appointment.EndsAt = endsAt;
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Cita", appointment.Id.ToString(),
                                             $"El paciente {patient.Name} reprogramó su cita del {previous:dd/MM/yyyy HH:mm} al {startsAt:dd/MM/yyyy HH:mm}");

                return Response.Success("Cita reprogramada con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        public async Task<List<string>> GetAvailableSlotsAsync(Guid doctorProfileId, Guid clinicalServiceId, DateOnly date)
        {
            ClinicalService? service = await _context.ClinicalServices.AsNoTracking()
                                                                      .FirstOrDefaultAsync(cs => cs.Id == clinicalServiceId && cs.IsActive);
            if (service is null)
            {
                return [];
            }

            DayOfWeek day = date.DayOfWeek;
            DateTime dayStart = date.ToDateTime(TimeOnly.MinValue);
            DateTime dayEnd = dayStart.AddDays(1);

            List<DoctorAvailability> blocks = await _context.DoctorAvailabilities.AsNoTracking()
                                                                                 .Where(a => a.DoctorProfileId == doctorProfileId && a.DayOfWeek == day)
                                                                                 .OrderBy(a => a.StartTime)
                                                                                 .ToListAsync();

            var busy = await _context.Appointments.AsNoTracking()
                                                  .Where(a => a.DoctorProfileId == doctorProfileId
                                                              && a.Status != AppointmentStatus.Cancelled
                                                              && a.StartsAt >= dayStart
                                                              && a.StartsAt < dayEnd)
                                                  .Select(a => new { a.StartsAt, a.EndsAt })
                                                  .ToListAsync();

            List<string> slots = [];
            foreach (DoctorAvailability block in blocks)
            {
                DateTime start = date.ToDateTime(block.StartTime);
                DateTime blockEnd = date.ToDateTime(block.EndTime);

                // Mientras la cita completa quepa dentro del bloque
                while (start.AddMinutes(service.DurationMinutes) <= blockEnd)
                {
                    DateTime end = start.AddMinutes(service.DurationMinutes);
                    bool isFree = start > DateTime.Now && !busy.Any(b => b.StartsAt < end && start < b.EndsAt);
                    if (isFree)
                    {
                        slots.Add(start.ToString("HH:mm"));
                    }

                    start = start.AddMinutes(SLOT_STEP_MINUTES);
                }
            }

            return slots;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.Dashboard;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Panel general de indicadores: solo conteos y porcentajes informativos (sin ingresos ni cálculos contables)
    public class DashboardService : IDashboardService
    {
        private const int MAX_RANGE_DAYS = 366;

        private readonly DataContext _context;

        public DashboardService(DataContext context)
        {
            _context = context;
        }

        public async Task<Response<DashboardDTO>> GetIndicatorsAsync(DateTime? from, DateTime? to)
        {
            try
            {
                // Por defecto: mes actual
                DateTime today = DateTime.Today;
                DateTime start = (from ?? new DateTime(today.Year, today.Month, 1)).Date;
                DateTime end = (to ?? start.AddMonths(1).AddDays(-1)).Date;

                if (end < start)
                {
                    (start, end) = (end, start);
                }

                if ((end - start).TotalDays > MAX_RANGE_DAYS)
                {
                    end = start.AddDays(MAX_RANGE_DAYS);
                }

                DateTime endExclusive = end.AddDays(1);

                // Citas del periodo (proyección liviana; los agrupamientos se hacen en memoria)
                var appointments = await _context.Appointments.AsNoTracking()
                                                              .Where(a => a.StartsAt >= start && a.StartsAt < endExclusive)
                                                              .Select(a => new
                                                              {
                                                                  a.DoctorProfileId,
                                                                  a.StartsAt,
                                                                  a.EndsAt,
                                                                  a.Status,
                                                                  SpecialtyName = a.DoctorProfile!.Specialty!.Name
                                                              }).ToListAsync();

                var activeAppointments = appointments.Where(a => a.Status != AppointmentStatus.Cancelled).ToList();

                // Doctores aprobados con cuenta activa y su disponibilidad semanal
                var doctors = await _context.DoctorProfiles.AsNoTracking()
                                                           .Where(d => d.Status == ApprovalStatus.Approved && d.User!.IsActive)
                                                           .Select(d => new
                                                           {
                                                               d.Id,
                                                               Name = d.User!.Name,
                                                               SpecialtyName = d.Specialty!.Name
                                                           }).ToListAsync();

                List<DoctorAvailability> availabilities = await _context.DoctorAvailabilities.AsNoTracking()
                                                                                             .Where(a => a.DoctorProfile!.Status == ApprovalStatus.Approved
                                                                                                         && a.DoctorProfile.User!.IsActive)
                                                                                             .ToListAsync();

                int availableMinutes = OccupancyHelper.AvailableMinutes(availabilities, start, end);
                int bookedMinutes = activeAppointments.Sum(a => (int)(a.EndsAt - a.StartsAt).TotalMinutes);

                DashboardDTO dto = new DashboardDTO
                {
                    From = start,
                    To = end,

                    AppointmentsToday = await _context.Appointments.CountAsync(a => a.StartsAt >= today
                                                                                    && a.StartsAt < today.AddDays(1)
                                                                                    && a.Status != AppointmentStatus.Cancelled),
                    AppointmentsInRange = appointments.Count,
                    ScheduledInRange = appointments.Count(a => a.Status == AppointmentStatus.Scheduled),
                    AttendedInRange = appointments.Count(a => a.Status == AppointmentStatus.Attended),
                    CancelledInRange = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                    NoShowInRange = appointments.Count(a => a.Status == AppointmentStatus.NoShow),

                    AvailableMinutes = availableMinutes,
                    BookedMinutes = bookedMinutes,
                    OccupancyPercent = OccupancyHelper.Percent(bookedMinutes, availableMinutes),

                    ActiveDoctors = doctors.Count,
                    ActiveSecretaries = await _context.Users.CountAsync(u => u.IsActive && u.Role!.Name == Env.SECRETARY_ROLE_NAME),
                    ActivePatients = await _context.Users.CountAsync(u => u.IsActive && u.Role!.Name == Env.PATIENT_ROLE_NAME),
                    ClinicalRecords = await _context.ClinicalRecords.CountAsync(),
                    ActiveServices = await _context.ClinicalServices.CountAsync(cs => cs.IsActive),
                    PendingDoctors = await _context.DoctorProfiles.CountAsync(d => d.Status == ApprovalStatus.Pending),
                    PendingSpecialties = await _context.Specialties.CountAsync(s => s.Status == ApprovalStatus.Pending),
                };

                // Citas por día (todas las fechas del periodo, incluso sin citas)
                Dictionary<DateTime, int> byDay = activeAppointments.GroupBy(a => a.StartsAt.Date)
                                                                    .ToDictionary(g => g.Key, g => g.Count());

                for (DateTime day = start; day <= end; day = day.AddDays(1))
                {
                    dto.DailyAppointments.Add(new LabelValueDTO
                    {
                        Label = day.ToString("dd/MM"),
                        Value = byDay.TryGetValue(day, out int count) ? count : 0
                    });
                }

                dto.AppointmentsBySpecialty = activeAppointments.GroupBy(a => a.SpecialtyName)
                                                                .Select(g => new LabelValueDTO { Label = g.Key, Value = g.Count() })
                                                                .OrderByDescending(x => x.Value)
                                                                .ToList();

                dto.DoctorsOccupancy = doctors.Select(d =>
                {
                    int doctorAvailable = OccupancyHelper.AvailableMinutes(availabilities.Where(a => a.DoctorProfileId == d.Id), start, end);
                    var doctorAppointments = activeAppointments.Where(a => a.DoctorProfileId == d.Id).ToList();
                    int doctorBooked = doctorAppointments.Sum(a => (int)(a.EndsAt - a.StartsAt).TotalMinutes);

                    return new DoctorOccupancyDTO
                    {
                        DoctorName = d.Name,
                        SpecialtyName = d.SpecialtyName,
                        Appointments = doctorAppointments.Count,
                        Attended = doctorAppointments.Count(a => a.Status == AppointmentStatus.Attended),
                        AvailableMinutes = doctorAvailable,
                        BookedMinutes = doctorBooked,
                        OccupancyPercent = OccupancyHelper.Percent(doctorBooked, doctorAvailable)
                    };
                }).OrderByDescending(d => d.OccupancyPercent)
                  .ToList();

                return Response<DashboardDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<DashboardDTO>.Failure(ex);
            }
        }
    }
}

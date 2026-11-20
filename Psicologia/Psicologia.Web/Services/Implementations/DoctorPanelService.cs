using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.Appointment;
using Psicologia.Web.DTOs.DoctorPanel;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Panel informativo del doctor: atenciones y tarifas de referencia.
    // No realiza liquidaciones, cálculo de comisiones ni sumas de dinero.
    public class DoctorPanelService : IDoctorPanelService
    {
        private const int UPCOMING_SIZE = 5;

        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;

        public DoctorPanelService(DataContext context, IMapper mapper, IAccountService accountService)
        {
            _context = context;
            _mapper = mapper;
            _accountService = accountService;
        }

        public async Task<Response<DoctorPanelDTO>> GetSummaryAsync(Guid? doctorProfileId, DateTime? from, DateTime? to)
        {
            try
            {
                DoctorProfile? ownProfile = await _accountService.GetAccountDoctorProfileAsync();

                DateTime today = DateTime.Today;
                DateTime start = (from ?? new DateTime(today.Year, today.Month, 1)).Date;
                DateTime end = (to ?? start.AddMonths(1).AddDays(-1)).Date;

                if (end < start)
                {
                    (start, end) = (end, start);
                }

                DoctorPanelDTO dto = new DoctorPanelDTO
                {
                    CanSelectDoctor = ownProfile is null,
                    From = start,
                    To = end
                };

                Guid? targetId = ownProfile?.Id ?? doctorProfileId;

                if (targetId is null)
                {
                    targetId = await _context.DoctorProfiles.OrderBy(d => d.User!.Name)
                                                            .Select(d => (Guid?)d.Id)
                                                            .FirstOrDefaultAsync();
                }

                if (targetId is null)
                {
                    return Response<DoctorPanelDTO>.Success(dto);
                }

                DoctorProfile? profile = await _context.DoctorProfiles.AsNoTracking()
                                                                      .Include(d => d.User)
                                                                      .Include(d => d.Specialty)
                                                                      .FirstOrDefaultAsync(d => d.Id == targetId);
                if (profile is null)
                {
                    return Response<DoctorPanelDTO>.Failure("No existe el doctor seleccionado");
                }

                DateTime endExclusive = end.AddDays(1);

                var appointments = await _context.Appointments.AsNoTracking()
                                                              .Where(a => a.DoctorProfileId == profile.Id
                                                                          && a.StartsAt >= start
                                                                          && a.StartsAt < endExclusive)
                                                              .Select(a => new
                                                              {
                                                                  a.PatientId,
                                                                  a.ClinicalServiceId,
                                                                  a.StartsAt,
                                                                  a.EndsAt,
                                                                  a.Status
                                                              }).ToListAsync();

                List<DoctorAvailability> availabilities = await _context.DoctorAvailabilities.AsNoTracking()
                                                                                             .Where(a => a.DoctorProfileId == profile.Id)
                                                                                             .ToListAsync();

                int availableMinutes = OccupancyHelper.AvailableMinutes(availabilities, start, end);
                int bookedMinutes = appointments.Where(a => a.Status != AppointmentStatus.Cancelled)
                                                .Sum(a => (int)(a.EndsAt - a.StartsAt).TotalMinutes);

                dto.DoctorProfileId = profile.Id;
                dto.DoctorName = profile.User?.Name;
                dto.SpecialtyName = profile.Specialty?.Name;
                dto.ProfessionalLicense = profile.ProfessionalLicense;
                dto.Status = profile.Status;

                dto.TotalAppointments = appointments.Count;
                dto.Attended = appointments.Count(a => a.Status == AppointmentStatus.Attended);
                dto.Scheduled = appointments.Count(a => a.Status == AppointmentStatus.Scheduled);
                dto.Cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
                dto.NoShow = appointments.Count(a => a.Status == AppointmentStatus.NoShow);
                dto.PatientsAttended = appointments.Where(a => a.Status == AppointmentStatus.Attended)
                                                   .Select(a => a.PatientId)
                                                   .Distinct()
                                                   .Count();

                dto.NotesWritten = await _context.ClinicalNotes.CountAsync(n => n.DoctorProfileId == profile.Id
                                                                                && n.SessionDate >= start
                                                                                && n.SessionDate < endExclusive);

                dto.ActiveDiagnoses = await _context.Diagnoses.CountAsync(d => d.DoctorProfileId == profile.Id
                                                                               && (d.Status == DiagnosisStatus.Presumptive || d.Status == DiagnosisStatus.Confirmed));

                dto.WeeklyAvailableMinutes = OccupancyHelper.WeeklyMinutes(availabilities);
                dto.OccupancyPercent = OccupancyHelper.Percent(bookedMinutes, availableMinutes);

                // Servicios que puede prestar el doctor (de su especialidad o generales) con su tarifa de referencia
                List<ClinicalService> services = await _context.ClinicalServices.AsNoTracking()
                                                                                .Where(cs => cs.IsActive
                                                                                             && (cs.SpecialtyId == null || cs.SpecialtyId == profile.SpecialtyId))
                                                                                .OrderBy(cs => cs.Name)
                                                                                .ToListAsync();

                dto.Services = services.Select(cs => new DoctorServiceSummaryDTO
                {
                    ServiceName = cs.Name,
                    DurationMinutes = cs.DurationMinutes,
                    ReferenceRate = cs.ReferenceRate,
                    AttendedInRange = appointments.Count(a => a.ClinicalServiceId == cs.Id && a.Status == AppointmentStatus.Attended)
                }).ToList();

                List<Appointment> upcoming = await _context.Appointments.AsNoTracking()
                                                                        .Include(a => a.Patient)
                                                                        .Include(a => a.ClinicalService)
                                                                        .Include(a => a.DoctorProfile)
                                                                            .ThenInclude(d => d!.User)
                                                                        .Where(a => a.DoctorProfileId == profile.Id
                                                                                    && a.Status == AppointmentStatus.Scheduled
                                                                                    && a.StartsAt >= DateTime.Now)
                                                                        .OrderBy(a => a.StartsAt)
                                                                        .Take(UPCOMING_SIZE)
                                                                        .ToListAsync();

                dto.UpcomingAppointments = _mapper.Map<List<AppointmentDTO>>(upcoming);

                return Response<DoctorPanelDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<DoctorPanelDTO>.Failure(ex);
            }
        }
    }
}

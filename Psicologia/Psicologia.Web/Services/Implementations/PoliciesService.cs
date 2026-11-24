using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Extensions;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.Policy;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Políticas informativas: horario de atención y tarifas de referencia (sin lógica contable)
    public class PoliciesService : IPoliciesService
    {
        private readonly DataContext _context;
        private readonly IAuditService _auditService;

        public PoliciesService(DataContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<Response<PoliciesDTO>> GetPoliciesAsync()
        {
            try
            {
                List<ReferenceRateDTO> referenceRates = await _context.ClinicalServices.AsNoTracking()
                                                                                       .OrderByDescending(cs => cs.IsActive)
                                                                                       .ThenBy(cs => cs.Name)
                                                                                       .Select(cs => new ReferenceRateDTO
                {
                    ClinicalServiceId = cs.Id,
                    ServiceName = cs.Name,
                    SpecialtyName = cs.Specialty != null ? cs.Specialty.Name : null,
                    DurationMinutes = cs.DurationMinutes,
                    IsActive = cs.IsActive,
                    ReferenceRate = cs.ReferenceRate
                }).ToListAsync();

                PoliciesDTO dto = new PoliciesDTO
                {
                    BusinessHours = await GetBusinessHoursAsync(),
                    ReferenceRates = referenceRates
                };

                return Response<PoliciesDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<PoliciesDTO>.Failure(ex);
            }
        }

        // Siempre devuelve los 7 días (lunes a domingo); los días sin registro se muestran cerrados
        public async Task<List<BusinessHourDTO>> GetBusinessHoursAsync()
        {
            List<BusinessHour> hours = await _context.BusinessHours.AsNoTracking().ToListAsync();

            return DateExtensions.WeekDays.Select(day =>
            {
                BusinessHour? hour = hours.FirstOrDefault(h => h.DayOfWeek == day);

                return new BusinessHourDTO
                {
                    Id = hour?.Id ?? Guid.Empty,
                    DayOfWeek = day,
                    IsOpen = hour?.IsOpen ?? false,
                    OpenTime = hour?.OpenTime ?? new TimeOnly(8, 0),
                    CloseTime = hour?.CloseTime ?? new TimeOnly(18, 0)
                };
            }).ToList();
        }

        public async Task<Response> UpdateBusinessHoursAsync(List<BusinessHourDTO> businessHours)
        {
            try
            {
                if (businessHours.Count == 0)
                {
                    return Response.Failure("No se recibieron horarios para actualizar");
                }

                foreach (BusinessHourDTO item in businessHours.Where(b => b.IsOpen))
                {
                    if (item.OpenTime >= item.CloseTime)
                    {
                        return Response.Failure($"{item.DayOfWeek.ToSpanish()}: la hora de apertura debe ser menor que la de cierre");
                    }
                }

                List<BusinessHour> current = await _context.BusinessHours.ToListAsync();

                foreach (BusinessHourDTO item in businessHours)
                {
                    BusinessHour? hour = current.FirstOrDefault(h => h.DayOfWeek == item.DayOfWeek);

                    if (hour is null)
                    {
                        hour = new BusinessHour { Id = Guid.NewGuid(), DayOfWeek = item.DayOfWeek };
                        await _context.BusinessHours.AddAsync(hour);
                    }

                    hour.IsOpen = item.IsOpen;
                    hour.OpenTime = item.OpenTime;
                    hour.CloseTime = item.CloseTime;
                }

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Políticas", null, "Se actualizó el horario de atención de la clínica");

                // Informativo: bloques de agenda de doctores que quedan por fuera del nuevo horario
                int outOfHours = await CountAvailabilitiesOutOfHoursAsync();

                string message = "Horario de atención actualizado con éxito";

                if (outOfHours > 0)
                {
                    message += $". Atención: {outOfHours} bloque(s) de disponibilidad de doctores quedan fuera del nuevo horario";
                }

                return Response.Success(message);
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        public async Task<Response> UpdateReferenceRatesAsync(List<ReferenceRateDTO> referenceRates)
        {
            try
            {
                if (referenceRates.Any(r => r.ReferenceRate < 0))
                {
                    return Response.Failure("Las tarifas de referencia no pueden ser negativas");
                }

                HashSet<Guid> ids = referenceRates.Select(r => r.ClinicalServiceId).ToHashSet();

                // El catálogo de servicios es pequeño: se carga completo y se filtra en memoria
                List<ClinicalService> services = await _context.ClinicalServices.ToListAsync();

                int changed = 0;

                foreach (ClinicalService service in services.Where(s => ids.Contains(s.Id)))
                {
                    decimal newRate = referenceRates.First(r => r.ClinicalServiceId == service.Id).ReferenceRate;

                    if (service.ReferenceRate != newRate)
                    {
                        service.ReferenceRate = newRate;
                        changed++;
                    }
                }

                await _context.SaveChangesAsync();

                if (changed > 0)
                {
                    await _auditService.LogAsync(AuditActions.UPDATE, "Políticas", null, $"Se actualizaron {changed} tarifa(s) de referencia");
                }

                return Response.Success("Tarifas de referencia actualizadas con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        private async Task<int> CountAvailabilitiesOutOfHoursAsync()
        {
            List<BusinessHour> hours = await _context.BusinessHours.AsNoTracking().ToListAsync();
            List<DoctorAvailability> availabilities = await _context.DoctorAvailabilities.AsNoTracking().ToListAsync();

            return availabilities.Count(a =>
            {
                BusinessHour? hour = hours.FirstOrDefault(h => h.DayOfWeek == a.DayOfWeek);
                return hour is null || !hour.IsOpen || a.StartTime < hour.OpenTime || a.EndTime > hour.CloseTime;
            });
        }
    }
}

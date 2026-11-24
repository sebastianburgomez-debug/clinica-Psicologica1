using Psicologia.Web.Core;
using Psicologia.Web.DTOs.Policy;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IPoliciesService
    {
        public Task<Response<PoliciesDTO>> GetPoliciesAsync();
        public Task<List<BusinessHourDTO>> GetBusinessHoursAsync();
        public Task<Response> UpdateBusinessHoursAsync(List<BusinessHourDTO> businessHours);
        public Task<Response> UpdateReferenceRatesAsync(List<ReferenceRateDTO> referenceRates);
    }
}

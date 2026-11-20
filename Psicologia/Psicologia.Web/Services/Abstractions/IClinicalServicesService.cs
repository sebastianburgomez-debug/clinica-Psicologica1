using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.ClinicalService;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IClinicalServicesService
    {
        public Task<Response<CreateClinicalServiceDTO>> CreateAsync(CreateClinicalServiceDTO dto);
        public Task<Response> DeleteAsync(Guid id);
        public Task<Response<ClinicalServiceDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<ClinicalServiceDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<ClinicalServiceDTO>> UpdateAsync(UpdateClinicalServiceDTO dto);
        public Task<Response> ToggleAsync(ToggleClinicalServiceStatusDTO dto);
    }
}

using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Specialty;

namespace Psicologia.Web.Services.Abstractions
{
    public interface ISpecialtiesService
    {
        public Task<Response<CreateSpecialtyDTO>> CreateAsync(CreateSpecialtyDTO dto);
        public Task<Response> DeleteAsync(Guid id);
        public Task<Response<SpecialtyDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<SpecialtyDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<SpecialtyDTO>> UpdateAsync(UpdateSpecialtyDTO dto);
    }
}

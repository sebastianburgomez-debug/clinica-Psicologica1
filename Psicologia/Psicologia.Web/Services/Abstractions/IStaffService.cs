using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Staff;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IStaffService
    {
        public Task<Response<PaginationResponse<StaffDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<UpdateStaffDTO>> GetOneAsync(Guid id);
        public Task<Response<StaffDTO>> CreateAsync(CreateStaffDTO dto);
        public Task<Response<StaffDTO>> UpdateAsync(UpdateStaffDTO dto);
        public Task<Response> ToggleAsync(ToggleStaffStatusDTO dto);
    }
}

using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Waitlist;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IWaitlistService
    {
        public Task<Response<PaginationResponse<WaitlistEntryDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response> CreateAsync(CreateWaitlistEntryDTO dto);
        public Task<Response<AssignWaitlistEntryDTO>> GetForAssignAsync(Guid id);
        public Task<Response> AssignAsync(AssignWaitlistEntryDTO dto);
        public Task<Response> CancelAsync(Guid id);
    }
}

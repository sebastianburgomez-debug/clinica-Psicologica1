using Psicologia.Web.Core;
using Psicologia.Web.DTOs.Approval;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IApprovalsService
    {
        public Task<Response<ApprovalsDTO>> GetApprovalsAsync();
        public Task<Response> ReviewDoctorAsync(ReviewRequestDTO dto);
        public Task<Response> ReviewSpecialtyAsync(ReviewRequestDTO dto);
    }
}

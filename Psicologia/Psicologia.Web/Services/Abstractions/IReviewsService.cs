using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Review;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IReviewsService
    {
        public Task<Response<PaginationResponse<ReviewDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<CreateReviewDTO>> GetForCreateAsync(Guid appointmentId);
        public Task<Response> CreateAsync(CreateReviewDTO dto);
    }
}

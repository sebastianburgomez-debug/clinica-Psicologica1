using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Appointment;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IAppointmentsService
    {
        public Task<Response<PaginationResponse<AppointmentDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<AppointmentDTO>> CreateAsync(CreateAppointmentDTO dto);
        public Task<Response> ChangeStatusAsync(ChangeAppointmentStatusDTO dto);
    }
}

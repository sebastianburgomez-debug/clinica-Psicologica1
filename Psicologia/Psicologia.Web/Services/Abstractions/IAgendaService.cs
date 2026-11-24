using Psicologia.Web.Core;
using Psicologia.Web.DTOs.Agenda;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IAgendaService
    {
        // doctorProfileId solo se tiene en cuenta cuando quien consulta no es doctor
        public Task<Response<AgendaDTO>> GetAgendaAsync(Guid? doctorProfileId, DateTime? weekStart);
        public Task<Response> CreateAvailabilityAsync(CreateAvailabilityDTO dto);
        public Task<Response<Guid>> DeleteAvailabilityAsync(Guid id);
    }
}

using Psicologia.Web.Core;
using Psicologia.Web.DTOs.DoctorPanel;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IDoctorPanelService
    {
        // doctorProfileId solo se tiene en cuenta cuando quien consulta no es doctor
        public Task<Response<DoctorPanelDTO>> GetSummaryAsync(Guid? doctorProfileId, DateTime? from, DateTime? to);
    }
}

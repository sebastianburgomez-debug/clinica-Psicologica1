using Psicologia.Web.Core;
using Psicologia.Web.DTOs.Dashboard;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IDashboardService
    {
        public Task<Response<DashboardDTO>> GetIndicatorsAsync(DateTime? from, DateTime? to);
    }
}

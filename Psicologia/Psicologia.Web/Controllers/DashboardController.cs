using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.DTOs.Dashboard;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Panel de Dirección: indicadores generales (informativo)
    public class DashboardController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IDashboardService _service;

        public DashboardController(INotyfService notyfService, IDashboardService service)
        {
            _notyfService = notyfService;
            _service = service;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "dashboard.show")]
        public async Task<IActionResult> Index([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            Response<DashboardDTO> response = await _service.GetIndicatorsAsync(from, to);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }
    }
}

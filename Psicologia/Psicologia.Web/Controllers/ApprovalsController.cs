using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.DTOs.Approval;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Panel de Dirección: aprobación de nuevos doctores y especialidades
    public class ApprovalsController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IApprovalsService _service;

        public ApprovalsController(INotyfService notyfService, IApprovalsService service)
        {
            _notyfService = notyfService;
            _service = service;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "approvals.show")]
        public async Task<IActionResult> Index()
        {
            Response<ApprovalsDTO> response = await _service.GetApprovalsAsync();

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "approvals.manage")]
        public async Task<IActionResult> ReviewDoctor([FromForm] ReviewRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return RedirectToAction(nameof(Index));
            }

            Response response = await _service.ReviewDoctorAsync(dto);

            NotifyResult(response);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "approvals.manage")]
        public async Task<IActionResult> ReviewSpecialty([FromForm] ReviewRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return RedirectToAction(nameof(Index));
            }

            Response response = await _service.ReviewSpecialtyAsync(dto);

            NotifyResult(response);

            return RedirectToAction(nameof(Index));
        }

        private void NotifyResult(Response response)
        {
            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
            }
            else
            {
                _notyfService.Success(response.Message);
            }
        }
    }
}

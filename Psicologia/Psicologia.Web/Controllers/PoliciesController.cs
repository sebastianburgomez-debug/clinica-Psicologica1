using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.DTOs.Policy;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Panel de Dirección: horario de atención y tarifas de referencia (informativo)
    public class PoliciesController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IPoliciesService _service;

        public PoliciesController(INotyfService notyfService, IPoliciesService service)
        {
            _notyfService = notyfService;
            _service = service;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "policies.show")]
        public async Task<IActionResult> Index()
        {
            Response<PoliciesDTO> response = await _service.GetPoliciesAsync();

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "policies.update")]
        public async Task<IActionResult> UpdateBusinessHours([FromForm] PoliciesDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return RedirectToAction(nameof(Index));
            }

            Response response = await _service.UpdateBusinessHoursAsync(dto.BusinessHours);

            NotifyResult(response);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "policies.update")]
        public async Task<IActionResult> UpdateReferenceRates([FromForm] PoliciesDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return RedirectToAction(nameof(Index));
            }

            Response response = await _service.UpdateReferenceRatesAsync(dto.ReferenceRates);

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

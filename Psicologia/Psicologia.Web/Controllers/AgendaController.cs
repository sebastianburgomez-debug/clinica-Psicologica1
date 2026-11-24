using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.DTOs.Agenda;
using Psicologia.Web.Helpers.Abstractions;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Atención Clínica: agenda personal (disponibilidad horaria de cada doctor)
    public class AgendaController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IAgendaService _service;
        private readonly ISelectListHelper _selectListHelper;

        public AgendaController(INotyfService notyfService, IAgendaService service, ISelectListHelper selectListHelper)
        {
            _notyfService = notyfService;
            _service = service;
            _selectListHelper = selectListHelper;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "agenda.show")]
        public async Task<IActionResult> Index([FromQuery] Guid? doctorId, [FromQuery] DateTime? week)
        {
            Response<AgendaDTO> response = await _service.GetAgendaAsync(doctorId, week);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            AgendaDTO dto = response.Result;

            if (dto.CanSelectDoctor)
            {
                dto.Doctors = await _selectListHelper.GetDoctorsSelectListAsync(onlyApproved: false);
            }

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "agenda.manage")]
        public async Task<IActionResult> CreateAvailability([FromForm][Bind(Prefix = nameof(AgendaDTO.NewAvailability))] CreateAvailabilityDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return RedirectToAction(nameof(Index), new { doctorId = dto.DoctorProfileId });
            }

            Response response = await _service.CreateAvailabilityAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
            }
            else
            {
                _notyfService.Success(response.Message);
            }

            return RedirectToAction(nameof(Index), new { doctorId = dto.DoctorProfileId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "agenda.manage")]
        public async Task<IActionResult> DeleteAvailability([FromRoute] Guid id)
        {
            Response<Guid> response = await _service.DeleteAvailabilityAsync(id);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index), new { doctorId = response.Result });
        }
    }
}

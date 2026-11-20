using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.DTOs.DoctorPanel;
using Psicologia.Web.Helpers.Abstractions;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Atención Clínica: panel informativo del doctor (sin liquidaciones ni comisiones)
    public class DoctorPanelController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IDoctorPanelService _service;
        private readonly ISelectListHelper _selectListHelper;

        public DoctorPanelController(INotyfService notyfService, IDoctorPanelService service, ISelectListHelper selectListHelper)
        {
            _notyfService = notyfService;
            _service = service;
            _selectListHelper = selectListHelper;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "doctorpanel.show")]
        public async Task<IActionResult> Index([FromQuery] Guid? doctorId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            Response<DoctorPanelDTO> response = await _service.GetSummaryAsync(doctorId, from, to);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            DoctorPanelDTO dto = response.Result;

            if (dto.CanSelectDoctor)
            {
                dto.Doctors = await _selectListHelper.GetDoctorsSelectListAsync(onlyApproved: false);
            }

            return View(dto);
        }
    }
}

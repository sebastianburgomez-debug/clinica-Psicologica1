using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Waitlist;
using Psicologia.Web.Helpers.Abstractions;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Atención Clínica: lista de espera gestionada por la secretaria
    public class WaitlistController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IWaitlistService _service;
        private readonly ISelectListHelper _selectListHelper;

        public WaitlistController(INotyfService notyfService,
                                  IWaitlistService service,
                                  ISelectListHelper selectListHelper)
        {
            _notyfService = notyfService;
            _service = service;
            _selectListHelper = selectListHelper;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "waitlist.show")]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<WaitlistEntryDTO>> response = await _service.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "waitlist.manage")]
        public async Task<IActionResult> Create()
        {
            CreateWaitlistEntryDTO dto = new CreateWaitlistEntryDTO();
            await LoadSelectListsAsync(dto);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "waitlist.manage")]
        public async Task<IActionResult> Create([FromForm] CreateWaitlistEntryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadSelectListsAsync(dto);
                return View(dto);
            }

            Response response = await _service.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                await LoadSelectListsAsync(dto);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "waitlist.manage")]
        public async Task<IActionResult> Assign([FromRoute] Guid id)
        {
            Response<AssignWaitlistEntryDTO> response = await _service.GetForAssignAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction(nameof(Index));
            }

            response.Result.Doctors = await _selectListHelper.GetDoctorsSelectListAsync();
            return View(response.Result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "waitlist.manage")]
        public async Task<IActionResult> Assign([FromForm] AssignWaitlistEntryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return await ReloadAssignViewAsync(dto);
            }

            Response response = await _service.AssignAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return await ReloadAssignViewAsync(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "waitlist.manage")]
        public async Task<IActionResult> Cancel([FromForm] Guid id)
        {
            Response response = await _service.CancelAsync(id);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
            }
            else
            {
                _notyfService.Success(response.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        // Si la asignación falla se recargan los datos del registro (paciente y servicio no viajan en el formulario)
        private async Task<IActionResult> ReloadAssignViewAsync(AssignWaitlistEntryDTO posted)
        {
            Response<AssignWaitlistEntryDTO> response = await _service.GetForAssignAsync(posted.Id);

            if (!response.IsSuccess || response.Result is null)
            {
                return RedirectToAction(nameof(Index));
            }

            response.Result.DoctorProfileId = posted.DoctorProfileId;
            response.Result.Date = posted.Date;
            response.Result.StartTime = posted.StartTime;
            response.Result.Doctors = await _selectListHelper.GetDoctorsSelectListAsync();
            return View(response.Result);
        }

        private async Task LoadSelectListsAsync(CreateWaitlistEntryDTO dto)
        {
            dto.Patients = await _selectListHelper.GetPatientsSelectListAsync();
            dto.Services = await _selectListHelper.GetClinicalServicesSelectListAsync();
            dto.Doctors = await _selectListHelper.GetDoctorsSelectListAsync();
        }
    }
}

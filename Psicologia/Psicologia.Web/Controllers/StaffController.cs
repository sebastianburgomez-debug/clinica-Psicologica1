using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Staff;
using Psicologia.Web.Helpers.Abstractions;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Administración del Sistema: gestión de doctores y secretarias (CRUD + borrado lógico)
    public class StaffController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IStaffService _service;
        private readonly ISelectListHelper _selectListHelper;

        public StaffController(INotyfService notyfService, IStaffService service, ISelectListHelper selectListHelper)
        {
            _notyfService = notyfService;
            _service = service;
            _selectListHelper = selectListHelper;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "staff.show")]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<StaffDTO>> response = await _service.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "staff.create")]
        public async Task<IActionResult> Create()
        {
            CreateStaffDTO dto = new CreateStaffDTO
            {
                Specialties = await _selectListHelper.GetSpecialtiesSelectListAsync()
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "staff.create")]
        public async Task<IActionResult> Create([FromForm] CreateStaffDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                dto.Specialties = await _selectListHelper.GetSpecialtiesSelectListAsync();
                return View(dto);
            }

            Response<StaffDTO> response = await _service.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                dto.Specialties = await _selectListHelper.GetSpecialtiesSelectListAsync();
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "staff.update")]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<UpdateStaffDTO> response = await _service.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            UpdateStaffDTO dto = response.Result;
            dto.Specialties = await _selectListHelper.GetSpecialtiesSelectListAsync();

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "staff.update")]
        public async Task<IActionResult> Edit([FromForm] UpdateStaffDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                dto.Specialties = await _selectListHelper.GetSpecialtiesSelectListAsync();
                return View(dto);
            }

            Response<StaffDTO> response = await _service.UpdateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                dto.Specialties = await _selectListHelper.GetSpecialtiesSelectListAsync();
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "staff.toggle")]
        public async Task<IActionResult> Toggle([FromForm] ToggleStaffStatusDTO dto)
        {
            Response response = await _service.ToggleAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
            }
            else
            {
                _notyfService.Success(response.Message);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

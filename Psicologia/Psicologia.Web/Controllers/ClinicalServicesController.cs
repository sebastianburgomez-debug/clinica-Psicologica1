using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.ClinicalService;
using Psicologia.Web.Helpers.Abstractions;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Administración del Sistema: catálogo de servicios psicológicos
    public class ClinicalServicesController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IClinicalServicesService _service;
        private readonly ISelectListHelper _selectListHelper;

        public ClinicalServicesController(INotyfService notyfService, IClinicalServicesService service, ISelectListHelper selectListHelper)
        {
            _notyfService = notyfService;
            _service = service;
            _selectListHelper = selectListHelper;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "services.show")]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<ClinicalServiceDTO>> response = await _service.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "services.create")]
        public async Task<IActionResult> Create()
        {
            CreateClinicalServiceDTO dto = new CreateClinicalServiceDTO
            {
                Specialties = await _selectListHelper.GetSpecialtiesSelectListAsync()
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "services.create")]
        public async Task<IActionResult> Create([FromForm] CreateClinicalServiceDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                dto.Specialties = await _selectListHelper.GetSpecialtiesSelectListAsync();
                return View(dto);
            }

            Response<CreateClinicalServiceDTO> response = await _service.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                dto.Specialties = await _selectListHelper.GetSpecialtiesSelectListAsync();
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "services.update")]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<ClinicalServiceDTO> response = await _service.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            UpdateClinicalServiceDTO dto = new UpdateClinicalServiceDTO
            {
                Id = response.Result.Id,
                Name = response.Result.Name,
                Description = response.Result.Description,
                DurationMinutes = response.Result.DurationMinutes,
                ReferenceRate = response.Result.ReferenceRate,
                SpecialtyId = response.Result.SpecialtyId,
                Specialties = await _selectListHelper.GetSpecialtiesSelectListAsync()
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "services.update")]
        public async Task<IActionResult> Edit([FromForm] UpdateClinicalServiceDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                dto.Specialties = await _selectListHelper.GetSpecialtiesSelectListAsync();
                return View(dto);
            }

            Response<ClinicalServiceDTO> response = await _service.UpdateAsync(dto);

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
        [CustomAuthorize(permissionCode: "services.delete")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            Response response = await _service.DeleteAsync(id);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "services.update")]
        public async Task<IActionResult> Toggle([FromForm] ToggleClinicalServiceStatusDTO dto)
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

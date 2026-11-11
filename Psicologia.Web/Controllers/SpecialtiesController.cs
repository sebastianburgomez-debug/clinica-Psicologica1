using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Specialty;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    public class SpecialtiesController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly ISpecialtiesService _service;

        public SpecialtiesController(INotyfService notyfService, ISpecialtiesService service)
        {
            _notyfService = notyfService;
            _service = service;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "specialties.show")]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<SpecialtyDTO>> response = await _service.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "specialties.create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "specialties.create")]
        public async Task<IActionResult> Create([FromForm] CreateSpecialtyDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response<CreateSpecialtyDTO> response = await _service.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "specialties.update")]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<SpecialtyDTO> response = await _service.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            UpdateSpecialtyDTO dto = new UpdateSpecialtyDTO
            {
                Id = response.Result.Id,
                Name = response.Result.Name
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "specialties.update")]
        public async Task<IActionResult> Edit([FromForm] UpdateSpecialtyDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response<SpecialtyDTO> response = await _service.UpdateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "specialties.delete")]
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
    }
}

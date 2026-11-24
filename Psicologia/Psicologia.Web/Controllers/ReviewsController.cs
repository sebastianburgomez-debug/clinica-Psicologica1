using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Review;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Reseñas: el paciente califica sesiones atendidas; jefe, administrador y doctores consultan
    public class ReviewsController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IReviewsService _service;

        public ReviewsController(INotyfService notyfService, IReviewsService service)
        {
            _notyfService = notyfService;
            _service = service;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "reviews.show")]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<ReviewDTO>> response = await _service.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "reviews.create")]
        public async Task<IActionResult> Create([FromQuery] Guid appointmentId)
        {
            Response<CreateReviewDTO> response = await _service.GetForCreateAsync(appointmentId);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "PatientPortal");
            }

            return View(response.Result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "reviews.create")]
        public async Task<IActionResult> Create([FromForm] CreateReviewDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return await ReloadCreateViewAsync(dto);
            }

            Response response = await _service.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return await ReloadCreateViewAsync(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction("Index", "PatientPortal");
        }

        // Si la validación falla se recargan los datos de la sesión (no viajan en el formulario)
        private async Task<IActionResult> ReloadCreateViewAsync(CreateReviewDTO posted)
        {
            Response<CreateReviewDTO> response = await _service.GetForCreateAsync(posted.AppointmentId);

            if (!response.IsSuccess || response.Result is null)
            {
                return RedirectToAction("Index", "PatientPortal");
            }

            response.Result.Rating = posted.Rating;
            response.Result.Comment = posted.Comment;
            return View(response.Result);
        }
    }
}

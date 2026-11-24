using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.DTOs.Appointment;
using Psicologia.Web.DTOs.Portal;
using Psicologia.Web.Helpers.Abstractions;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Portal del paciente: citas propias (solicitar, reprogramar, cancelar) e historial
    public class PatientPortalController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IPatientPortalService _service;
        private readonly ISelectListHelper _selectListHelper;

        public PatientPortalController(INotyfService notyfService,
                                       IPatientPortalService service,
                                       ISelectListHelper selectListHelper)
        {
            _notyfService = notyfService;
            _service = service;
            _selectListHelper = selectListHelper;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "portal.show")]
        public async Task<IActionResult> Index()
        {
            Response<List<PatientAppointmentDTO>> response = await _service.GetMyAppointmentsAsync();

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "portal.manage")]
        public async Task<IActionResult> Book()
        {
            BookAppointmentDTO dto = new BookAppointmentDTO
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            };

            await LoadSelectListsAsync(dto);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "portal.manage")]
        public async Task<IActionResult> Book([FromForm] BookAppointmentDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadSelectListsAsync(dto);
                return View(dto);
            }

            Response<AppointmentDTO> response = await _service.BookAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                await LoadSelectListsAsync(dto);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        // Horas libres para el selector del formulario (se consulta con fetch desde la vista)
        [HttpGet]
        [CustomAuthorize(permissionCode: "portal.manage")]
        public async Task<IActionResult> Slots([FromQuery] Guid doctorId, [FromQuery] Guid serviceId, [FromQuery] DateOnly date)
        {
            List<string> slots = await _service.GetAvailableSlotsAsync(doctorId, serviceId, date);
            return Json(slots);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "portal.manage")]
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

        [HttpGet]
        [CustomAuthorize(permissionCode: "portal.manage")]
        public async Task<IActionResult> Reschedule([FromRoute] Guid id)
        {
            Response<RescheduleAppointmentDTO> response = await _service.GetForRescheduleAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction(nameof(Index));
            }

            return View(response.Result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "portal.manage")]
        public async Task<IActionResult> Reschedule([FromForm] RescheduleAppointmentDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return await ReloadRescheduleViewAsync(dto);
            }

            Response response = await _service.RescheduleAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return await ReloadRescheduleViewAsync(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        // Si la reprogramación falla se vuelve a cargar la cita para mostrar de nuevo sus datos
        private async Task<IActionResult> ReloadRescheduleViewAsync(RescheduleAppointmentDTO posted)
        {
            Response<RescheduleAppointmentDTO> response = await _service.GetForRescheduleAsync(posted.Id);

            if (!response.IsSuccess || response.Result is null)
            {
                return RedirectToAction(nameof(Index));
            }

            response.Result.Date = posted.Date;
            response.Result.StartTime = posted.StartTime;
            return View(response.Result);
        }

        private async Task LoadSelectListsAsync(BookAppointmentDTO dto)
        {
            dto.Doctors = await _selectListHelper.GetDoctorsSelectListAsync();
            dto.Services = await _selectListHelper.GetClinicalServicesSelectListAsync();
        }
    }
}

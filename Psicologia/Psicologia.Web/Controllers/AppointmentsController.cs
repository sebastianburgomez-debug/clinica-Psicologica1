using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.Appointment;
using Psicologia.Web.Helpers.Abstractions;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Atención Clínica: citas (base de los indicadores de ocupación)
    public class AppointmentsController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IAppointmentsService _service;
        private readonly IAccountService _accountService;
        private readonly ISelectListHelper _selectListHelper;

        public AppointmentsController(INotyfService notyfService,
                                      IAppointmentsService service,
                                      IAccountService accountService,
                                      ISelectListHelper selectListHelper)
        {
            _notyfService = notyfService;
            _service = service;
            _accountService = accountService;
            _selectListHelper = selectListHelper;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "appointments.show")]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<AppointmentDTO>> response = await _service.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "appointments.create")]
        public async Task<IActionResult> Create([FromQuery] Guid? doctorId)
        {
            DoctorProfile? ownProfile = await _accountService.GetAccountDoctorProfileAsync();
            User? accountUser = await _accountService.GetAccountUserAsync();

            CreateAppointmentDTO dto = new CreateAppointmentDTO
            {
                DoctorProfileId = ownProfile?.Id ?? doctorId,
                PatientId = (accountUser?.Role?.Name == Env.PATIENT_ROLE_NAME) ? accountUser.Id : null,
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = new TimeOnly(8, 0)
            };

            await LoadSelectListsAsync(dto);

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "appointments.create")]
        public async Task<IActionResult> Create([FromForm] CreateAppointmentDTO dto)
        {
            User? accountUser = await _accountService.GetAccountUserAsync();
            if (accountUser?.Role?.Name == Env.PATIENT_ROLE_NAME)
            {
                dto.PatientId = accountUser.Id;
                ModelState.Remove(nameof(dto.PatientId));
            }

            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                await LoadSelectListsAsync(dto);
                return View(dto);
            }

            Response<AppointmentDTO> response = await _service.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                await LoadSelectListsAsync(dto);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "appointments.update")]
        public async Task<IActionResult> ChangeStatus([FromForm] ChangeAppointmentStatusDTO dto)
        {
            Response response = await _service.ChangeStatusAsync(dto);

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

        private async Task LoadSelectListsAsync(CreateAppointmentDTO dto)
        {
            dto.Doctors = await _selectListHelper.GetDoctorsSelectListAsync();
            dto.Patients = await _selectListHelper.GetPatientsSelectListAsync();
            dto.Services = await _selectListHelper.GetClinicalServicesSelectListAsync();
        }
    }
}

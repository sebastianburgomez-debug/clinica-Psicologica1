using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.ClinicalRecord;
using Psicologia.Web.Helpers.Abstractions;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Atención Clínica: historia clínica (notas de sesión, evolución y diagnósticos)
    public class ClinicalRecordsController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IClinicalRecordsService _service;
        private readonly ISelectListHelper _selectListHelper;

        public ClinicalRecordsController(INotyfService notyfService, IClinicalRecordsService service, ISelectListHelper selectListHelper)
        {
            _notyfService = notyfService;
            _service = service;
            _selectListHelper = selectListHelper;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "clinicalrecords.show")]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<ClinicalRecordDTO>> response = await _service.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "clinicalrecords.show")]
        public async Task<IActionResult> Details([FromRoute] Guid id)
        {
            Response<ClinicalRecordDTO> response = await _service.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            return View(response.Result);
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "clinicalrecords.create")]
        public async Task<IActionResult> Create()
        {
            CreateClinicalRecordDTO dto = new CreateClinicalRecordDTO
            {
                Patients = await _selectListHelper.GetPatientsSelectListAsync(withoutClinicalRecord: true)
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "clinicalrecords.create")]
        public async Task<IActionResult> Create([FromForm] CreateClinicalRecordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                dto.Patients = await _selectListHelper.GetPatientsSelectListAsync(withoutClinicalRecord: true);
                return View(dto);
            }

            Response<ClinicalRecordDTO> response = await _service.CreateAsync(dto);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                dto.Patients = await _selectListHelper.GetPatientsSelectListAsync(withoutClinicalRecord: true);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Details), new { id = response.Result.Id });
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "clinicalrecords.update")]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<ClinicalRecordDTO> response = await _service.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            ClinicalRecordDTO record = response.Result;

            UpdateClinicalRecordDTO dto = new UpdateClinicalRecordDTO
            {
                Id = record.Id,
                PatientName = record.PatientName,
                DocumentNumber = record.DocumentNumber,
                BirthDate = record.BirthDate,
                Phone = record.Phone,
                ConsultationReason = record.ConsultationReason,
                Background = record.Background
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "clinicalrecords.update")]
        public async Task<IActionResult> Edit([FromForm] UpdateClinicalRecordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response response = await _service.UpdateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }

        // Crear (recordId) o editar (id) una nota clínica
        [HttpGet]
        [CustomAuthorize(permissionCode: "clinicalrecords.update")]
        public async Task<IActionResult> Note([FromQuery] Guid recordId, [FromQuery] Guid? id)
        {
            if (id is not null)
            {
                Response<SaveClinicalNoteDTO> noteResponse = await _service.GetNoteAsync(id.Value);

                if (!noteResponse.IsSuccess || noteResponse.Result is null)
                {
                    _notyfService.Error(noteResponse.Message);
                    return RedirectToAction(nameof(Details), new { id = recordId });
                }

                return View(noteResponse.Result);
            }

            Response<ClinicalRecordDTO> recordResponse = await _service.GetOneAsync(recordId);

            if (!recordResponse.IsSuccess || recordResponse.Result is null)
            {
                _notyfService.Error(recordResponse.Message);
                return RedirectToAction(nameof(Index));
            }

            if (!recordResponse.Result.CanWriteNotes)
            {
                _notyfService.Error("Solo los doctores aprobados pueden registrar notas clínicas");
                return RedirectToAction(nameof(Details), new { id = recordId });
            }

            SaveClinicalNoteDTO dto = new SaveClinicalNoteDTO
            {
                ClinicalRecordId = recordId,
                PatientName = recordResponse.Result.PatientName,
                SessionDate = DateTime.Now
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "clinicalrecords.update")]
        public async Task<IActionResult> Note([FromForm] SaveClinicalNoteDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response response = await _service.SaveNoteAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Details), new { id = dto.ClinicalRecordId });
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "clinicalrecords.update")]
        public async Task<IActionResult> AddDiagnosis([FromQuery] Guid recordId)
        {
            Response<ClinicalRecordDTO> recordResponse = await _service.GetOneAsync(recordId);

            if (!recordResponse.IsSuccess || recordResponse.Result is null)
            {
                _notyfService.Error(recordResponse.Message);
                return RedirectToAction(nameof(Index));
            }

            if (!recordResponse.Result.CanWriteNotes)
            {
                _notyfService.Error("Solo los doctores aprobados pueden registrar diagnósticos");
                return RedirectToAction(nameof(Details), new { id = recordId });
            }

            CreateDiagnosisDTO dto = new CreateDiagnosisDTO
            {
                ClinicalRecordId = recordId,
                PatientName = recordResponse.Result.PatientName
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "clinicalrecords.update")]
        public async Task<IActionResult> AddDiagnosis([FromForm] CreateDiagnosisDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response response = await _service.AddDiagnosisAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Details), new { id = dto.ClinicalRecordId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "clinicalrecords.update")]
        public async Task<IActionResult> UpdateDiagnosisStatus([FromForm] UpdateDiagnosisStatusDTO dto)
        {
            Response response = await _service.UpdateDiagnosisStatusAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
            }
            else
            {
                _notyfService.Success(response.Message);
            }

            return RedirectToAction(nameof(Details), new { id = dto.ClinicalRecordId });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Extensions;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.ClinicalRecord;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Historia clínica: notas de sesión, evolución y diagnósticos.
    // Reglas de acceso:
    //  - Administrador: consulta todas las historias (no escribe notas: no es doctor).
    //  - Doctor: solo las historias de pacientes que ha atendido/agendado o que él abrió.
    //  - Notas y diagnósticos: solo doctores aprobados; cada doctor edita únicamente lo que escribió.
    //  - La auditoría nunca guarda el contenido clínico, solo la acción realizada.
    public class ClinicalRecordsService : IClinicalRecordsService
    {
        private const string UPDATE_PERMISSION = "clinicalrecords.update";

        private readonly DataContext _context;
        private readonly IAccountService _accountService;
        private readonly IAuditService _auditService;

        public ClinicalRecordsService(DataContext context, IAccountService accountService, IAuditService auditService)
        {
            _context = context;
            _accountService = accountService;
            _auditService = auditService;
        }

        public async Task<Response<PaginationResponse<ClinicalRecordDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            try
            {
                AccessContext access = await GetAccessContextAsync();

                IQueryable<ClinicalRecord> query = ApplyScope(_context.ClinicalRecords.AsNoTracking(), access);

                if (!string.IsNullOrWhiteSpace(request.Filter))
                {
                    string filter = request.Filter.Trim().ToLower();

                    query = query.Where(r => r.Patient!.Name.ToLower().Contains(filter)
                                             || r.Patient!.Email!.ToLower().Contains(filter)
                                             || (r.DocumentNumber != null && r.DocumentNumber.ToLower().Contains(filter)));
                }

                IQueryable<ClinicalRecordDTO> projection = query.OrderBy(r => r.Patient!.Name)
                                                                .Select(r => new ClinicalRecordDTO
                {
                    Id = r.Id,
                    PatientId = r.PatientId,
                    PatientName = r.Patient!.Name,
                    PatientEmail = r.Patient!.Email,
                    DocumentNumber = r.DocumentNumber,
                    ConsultationReason = r.ConsultationReason,
                    CreatedAt = r.CreatedAt,
                    CreatedBy = r.CreatedBy,
                    NotesCount = r.Notes!.Count(),
                    LastSessionDate = r.Notes!.Max(n => (DateTime?)n.SessionDate)
                });

                PagedList<ClinicalRecordDTO> list = await PagedList<ClinicalRecordDTO>.ToPagedListAsync(projection, request);

                PaginationResponse<ClinicalRecordDTO> dto = new PaginationResponse<ClinicalRecordDTO>
                {
                    List = list,
                    CurrentPage = list.CurrentPage,
                    TotalPages = list.TotalPages,
                    RecordsPerPage = list.RecordsPerPage,
                    TotalCount = list.TotalCount,
                    Filter = request.Filter,
                };

                return Response<PaginationResponse<ClinicalRecordDTO>>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<PaginationResponse<ClinicalRecordDTO>>.Failure(ex);
            }
        }

        public async Task<Response<ClinicalRecordDTO>> GetOneAsync(Guid id)
        {
            try
            {
                AccessContext access = await GetAccessContextAsync();

                ClinicalRecord? record = await _context.ClinicalRecords.AsNoTracking()
                                                                       .Include(r => r.Patient)
                                                                       .Include(r => r.Notes!)
                                                                           .ThenInclude(n => n.DoctorProfile!)
                                                                               .ThenInclude(d => d.User)
                                                                       .Include(r => r.Diagnoses!)
                                                                           .ThenInclude(d => d.DoctorProfile!)
                                                                               .ThenInclude(d => d.User)
                                                                       .AsSplitQuery()
                                                                       .FirstOrDefaultAsync(r => r.Id == id);
                if (record is null)
                {
                    return Response<ClinicalRecordDTO>.Failure($"No existe historia clínica con id: {id}");
                }

                if (!await CanReadAsync(record, access))
                {
                    return Response<ClinicalRecordDTO>.Failure("No tiene acceso a esta historia clínica");
                }

                bool canUpdate = await _accountService.AccountUserIsAuthorizedAsync(UPDATE_PERMISSION);
                Guid? doctorId = access.Doctor?.Id;

                ClinicalRecordDTO dto = new ClinicalRecordDTO
                {
                    Id = record.Id,
                    PatientId = record.PatientId,
                    PatientName = record.Patient?.Name,
                    PatientEmail = record.Patient?.Email,
                    DocumentNumber = record.DocumentNumber,
                    BirthDate = record.BirthDate,
                    Phone = record.Phone,
                    ConsultationReason = record.ConsultationReason,
                    Background = record.Background,
                    CreatedAt = record.CreatedAt,
                    CreatedBy = record.CreatedBy,
                    UpdatedAt = record.UpdatedAt,
                    NotesCount = record.Notes?.Count() ?? 0,
                    LastSessionDate = record.Notes?.Max(n => (DateTime?)n.SessionDate),
                    CanEditRecord = canUpdate,
                    CanWriteNotes = canUpdate && access.IsApprovedDoctor,

                    Notes = (record.Notes ?? []).OrderByDescending(n => n.SessionDate)
                                                .Select(n => new ClinicalNoteDTO
                    {
                        Id = n.Id,
                        ClinicalRecordId = n.ClinicalRecordId,
                        Type = n.Type,
                        SessionDate = n.SessionDate,
                        Content = n.Content,
                        DoctorProfileId = n.DoctorProfileId,
                        DoctorName = n.DoctorProfile?.User?.Name,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt,
                        CanEdit = canUpdate && access.IsApprovedDoctor && n.DoctorProfileId == doctorId
                    }).ToList(),

                    Diagnoses = (record.Diagnoses ?? []).OrderBy(d => d.Status == DiagnosisStatus.Resolved || d.Status == DiagnosisStatus.Discarded)
                                                        .ThenByDescending(d => d.DiagnosedAt)
                                                        .Select(d => new DiagnosisDTO
                    {
                        Id = d.Id,
                        ClinicalRecordId = d.ClinicalRecordId,
                        Code = d.Code,
                        Name = d.Name,
                        Observations = d.Observations,
                        Status = d.Status,
                        DiagnosedAt = d.DiagnosedAt,
                        UpdatedAt = d.UpdatedAt,
                        DoctorName = d.DoctorProfile?.User?.Name,
                        CanEdit = canUpdate && access.IsApprovedDoctor && d.DoctorProfileId == doctorId
                    }).ToList()
                };

                return Response<ClinicalRecordDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<ClinicalRecordDTO>.Failure(ex);
            }
        }

        public async Task<Response<ClinicalRecordDTO>> CreateAsync(CreateClinicalRecordDTO dto)
        {
            try
            {
                User? patient = await _context.Users.Include(u => u.Role)
                                                    .FirstOrDefaultAsync(u => u.Id == dto.PatientId);

                if (patient is null || !patient.IsActive || patient.Role?.Name != Env.PATIENT_ROLE_NAME)
                {
                    return Response<ClinicalRecordDTO>.Failure("El paciente seleccionado no existe o está inactivo");
                }

                if (await _context.ClinicalRecords.AnyAsync(r => r.PatientId == patient.Id))
                {
                    return Response<ClinicalRecordDTO>.Failure($"El paciente '{patient.Name}' ya tiene una historia clínica");
                }

                User? accountUser = await _accountService.GetAccountUserAsync();

                ClinicalRecord record = new ClinicalRecord
                {
                    Id = Guid.NewGuid(),
                    PatientId = patient.Id,
                    DocumentNumber = dto.DocumentNumber?.Trim(),
                    BirthDate = dto.BirthDate,
                    Phone = dto.Phone?.Trim(),
                    ConsultationReason = dto.ConsultationReason.Trim(),
                    Background = dto.Background?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = accountUser?.Email
                };

                await _context.ClinicalRecords.AddAsync(record);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.CREATE, "Historia clínica", record.Id.ToString(), $"Se abrió la historia clínica de '{patient.Name}'");

                return Response<ClinicalRecordDTO>.Success(new ClinicalRecordDTO
                {
                    Id = record.Id,
                    PatientId = patient.Id,
                    PatientName = patient.Name
                }, "Historia clínica creada con éxito");
            }
            catch (Exception ex)
            {
                return Response<ClinicalRecordDTO>.Failure(ex);
            }
        }

        public async Task<Response> UpdateAsync(UpdateClinicalRecordDTO dto)
        {
            try
            {
                AccessContext access = await GetAccessContextAsync();

                ClinicalRecord? record = await _context.ClinicalRecords.Include(r => r.Patient)
                                                                       .FirstOrDefaultAsync(r => r.Id == dto.Id);
                if (record is null)
                {
                    return Response.Failure($"No existe historia clínica con id: {dto.Id}");
                }

                if (!await CanReadAsync(record, access))
                {
                    return Response.Failure("No tiene acceso a esta historia clínica");
                }

                record.DocumentNumber = dto.DocumentNumber?.Trim();
                record.BirthDate = dto.BirthDate;
                record.Phone = dto.Phone?.Trim();
                record.ConsultationReason = dto.ConsultationReason.Trim();
                record.Background = dto.Background?.Trim();
                record.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Historia clínica", record.Id.ToString(), $"Se actualizaron los datos generales de la historia de '{record.Patient?.Name}'");

                return Response.Success("Historia clínica actualizada con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        public async Task<Response<SaveClinicalNoteDTO>> GetNoteAsync(Guid noteId)
        {
            try
            {
                AccessContext access = await GetAccessContextAsync();

                ClinicalNote? note = await _context.ClinicalNotes.AsNoTracking()
                                                                 .Include(n => n.ClinicalRecord!)
                                                                     .ThenInclude(r => r.Patient)
                                                                 .FirstOrDefaultAsync(n => n.Id == noteId);
                if (note is null)
                {
                    return Response<SaveClinicalNoteDTO>.Failure($"No existe nota con id: {noteId}");
                }

                if (access.Doctor is null || note.DoctorProfileId != access.Doctor.Id)
                {
                    return Response<SaveClinicalNoteDTO>.Failure("Solo el doctor que escribió la nota puede editarla");
                }

                return Response<SaveClinicalNoteDTO>.Success(new SaveClinicalNoteDTO
                {
                    Id = note.Id,
                    ClinicalRecordId = note.ClinicalRecordId,
                    PatientName = note.ClinicalRecord?.Patient?.Name,
                    Type = note.Type,
                    SessionDate = note.SessionDate,
                    Content = note.Content
                });
            }
            catch (Exception ex)
            {
                return Response<SaveClinicalNoteDTO>.Failure(ex);
            }
        }

        public async Task<Response> SaveNoteAsync(SaveClinicalNoteDTO dto)
        {
            try
            {
                AccessContext access = await GetAccessContextAsync();

                if (!access.IsApprovedDoctor)
                {
                    return Response.Failure("Solo los doctores aprobados pueden registrar notas clínicas");
                }

                ClinicalRecord? record = await _context.ClinicalRecords.Include(r => r.Patient)
                                                                       .FirstOrDefaultAsync(r => r.Id == dto.ClinicalRecordId);
                if (record is null)
                {
                    return Response.Failure($"No existe historia clínica con id: {dto.ClinicalRecordId}");
                }

                if (!await CanReadAsync(record, access))
                {
                    return Response.Failure("No tiene acceso a esta historia clínica");
                }

                if (dto.SessionDate > DateTime.Now.AddMinutes(5))
                {
                    return Response.Failure("La fecha de la sesión no puede ser futura");
                }

                bool isNew = dto.Id is null || dto.Id == Guid.Empty;

                if (isNew)
                {
                    ClinicalNote note = new ClinicalNote
                    {
                        Id = Guid.NewGuid(),
                        ClinicalRecordId = record.Id,
                        DoctorProfileId = access.Doctor!.Id,
                        Type = dto.Type,
                        SessionDate = dto.SessionDate,
                        Content = dto.Content.Trim(),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.ClinicalNotes.AddAsync(note);
                    await _context.SaveChangesAsync();

                    await _auditService.LogAsync(AuditActions.CREATE, "Nota clínica", note.Id.ToString(),
                                                 $"{dto.Type.GetDisplayName()} registrada en la historia de '{record.Patient?.Name}'");

                    return Response.Success("Nota registrada con éxito");
                }

                ClinicalNote? existing = await _context.ClinicalNotes.FirstOrDefaultAsync(n => n.Id == dto.Id && n.ClinicalRecordId == record.Id);

                if (existing is null)
                {
                    return Response.Failure($"No existe nota con id: {dto.Id}");
                }

                if (existing.DoctorProfileId != access.Doctor!.Id)
                {
                    return Response.Failure("Solo el doctor que escribió la nota puede editarla");
                }

                existing.Type = dto.Type;
                existing.SessionDate = dto.SessionDate;
                existing.Content = dto.Content.Trim();
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Nota clínica", existing.Id.ToString(),
                                             $"{dto.Type.GetDisplayName()} modificada en la historia de '{record.Patient?.Name}'");

                return Response.Success("Nota actualizada con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        public async Task<Response> AddDiagnosisAsync(CreateDiagnosisDTO dto)
        {
            try
            {
                AccessContext access = await GetAccessContextAsync();

                if (!access.IsApprovedDoctor)
                {
                    return Response.Failure("Solo los doctores aprobados pueden registrar diagnósticos");
                }

                ClinicalRecord? record = await _context.ClinicalRecords.Include(r => r.Patient)
                                                                       .FirstOrDefaultAsync(r => r.Id == dto.ClinicalRecordId);
                if (record is null)
                {
                    return Response.Failure($"No existe historia clínica con id: {dto.ClinicalRecordId}");
                }

                if (!await CanReadAsync(record, access))
                {
                    return Response.Failure("No tiene acceso a esta historia clínica");
                }

                Diagnosis diagnosis = new Diagnosis
                {
                    Id = Guid.NewGuid(),
                    ClinicalRecordId = record.Id,
                    DoctorProfileId = access.Doctor!.Id,
                    Code = dto.Code?.Trim().ToUpper(),
                    Name = dto.Name.Trim(),
                    Observations = dto.Observations?.Trim(),
                    Status = dto.Status,
                    DiagnosedAt = dto.DiagnosedAt
                };

                await _context.Diagnoses.AddAsync(diagnosis);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.CREATE, "Diagnóstico", diagnosis.Id.ToString(),
                                             $"Diagnóstico registrado en la historia de '{record.Patient?.Name}'");

                return Response.Success("Diagnóstico registrado con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        public async Task<Response> UpdateDiagnosisStatusAsync(UpdateDiagnosisStatusDTO dto)
        {
            try
            {
                AccessContext access = await GetAccessContextAsync();

                Diagnosis? diagnosis = await _context.Diagnoses.Include(d => d.ClinicalRecord!)
                                                                   .ThenInclude(r => r.Patient)
                                                               .FirstOrDefaultAsync(d => d.Id == dto.Id && d.ClinicalRecordId == dto.ClinicalRecordId);
                if (diagnosis is null)
                {
                    return Response.Failure($"No existe diagnóstico con id: {dto.Id}");
                }

                if (!access.IsApprovedDoctor || diagnosis.DoctorProfileId != access.Doctor!.Id)
                {
                    return Response.Failure("Solo el doctor que registró el diagnóstico puede modificarlo");
                }

                diagnosis.Status = dto.Status;
                diagnosis.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Diagnóstico", diagnosis.Id.ToString(),
                                             $"Diagnóstico de '{diagnosis.ClinicalRecord?.Patient?.Name}' marcado como '{dto.Status.GetDisplayName()}'");

                return Response.Success("Diagnóstico actualizado con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        // ---------- Control de acceso ----------

        private sealed class AccessContext
        {
            public User? User { get; init; }
            public bool IsSuperAdmin { get; init; }
            public DoctorProfile? Doctor { get; init; }
            public bool IsApprovedDoctor => Doctor is not null && Doctor.Status == ApprovalStatus.Approved;
        }

        private async Task<AccessContext> GetAccessContextAsync()
        {
            User? user = await _accountService.GetAccountUserAsync();

            return new AccessContext
            {
                User = user,
                IsSuperAdmin = user?.Role?.Name == Env.SUPER_ADMIN_ROLE_NAME,
                Doctor = await _accountService.GetAccountDoctorProfileAsync()
            };
        }

        // Un doctor solo ve las historias de sus pacientes; el resto de perfiles autorizados ve todas
        private IQueryable<ClinicalRecord> ApplyScope(IQueryable<ClinicalRecord> query, AccessContext access)
        {
            if (access.IsSuperAdmin || access.Doctor is null)
            {
                return query;
            }

            Guid doctorId = access.Doctor.Id;
            string? email = access.User?.Email;

            return query.Where(r => r.CreatedBy == email
                                    || _context.Appointments.Any(a => a.PatientId == r.PatientId && a.DoctorProfileId == doctorId)
                                    || r.Notes!.Any(n => n.DoctorProfileId == doctorId)
                                    || r.Diagnoses!.Any(d => d.DoctorProfileId == doctorId));
        }

        private async Task<bool> CanReadAsync(ClinicalRecord record, AccessContext access)
        {
            if (access.User is null)
            {
                return false;
            }

            return await ApplyScope(_context.ClinicalRecords.AsNoTracking(), access).AnyAsync(r => r.Id == record.Id);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.Approval;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Flujo administrativo de revisión de doctores y nuevas especialidades
    public class ApprovalsService : IApprovalsService
    {
        private const int HISTORY_SIZE = 10;

        private readonly DataContext _context;
        private readonly IAccountService _accountService;
        private readonly IAuditService _auditService;

        public ApprovalsService(DataContext context, IAccountService accountService, IAuditService auditService)
        {
            _context = context;
            _accountService = accountService;
            _auditService = auditService;
        }

        public async Task<Response<ApprovalsDTO>> GetApprovalsAsync()
        {
            try
            {
                IQueryable<DoctorProfile> doctors = _context.DoctorProfiles.AsNoTracking()
                                                                           .Include(d => d.User)
                                                                           .Include(d => d.Specialty);

                IQueryable<Specialty> specialties = _context.Specialties.AsNoTracking();

                ApprovalsDTO dto = new ApprovalsDTO
                {
                    PendingDoctors = (await doctors.Where(d => d.Status == ApprovalStatus.Pending)
                                                   .OrderBy(d => d.RequestedAt)
                                                   .ToListAsync()).Select(ToDoctorDTO).ToList(),

                    PendingSpecialties = (await specialties.Where(s => s.Status == ApprovalStatus.Pending)
                                                           .OrderBy(s => s.RequestedAt)
                                                           .ToListAsync()).Select(ToSpecialtyDTO).ToList(),

                    ReviewedDoctors = (await doctors.Where(d => d.Status != ApprovalStatus.Pending && d.ReviewedAt != null)
                                                    .OrderByDescending(d => d.ReviewedAt)
                                                    .Take(HISTORY_SIZE)
                                                    .ToListAsync()).Select(ToDoctorDTO).ToList(),

                    ReviewedSpecialties = (await specialties.Where(s => s.Status != ApprovalStatus.Pending && s.ReviewedAt != null)
                                                            .OrderByDescending(s => s.ReviewedAt)
                                                            .Take(HISTORY_SIZE)
                                                            .ToListAsync()).Select(ToSpecialtyDTO).ToList()
                };

                return Response<ApprovalsDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<ApprovalsDTO>.Failure(ex);
            }
        }

        public async Task<Response> ReviewDoctorAsync(ReviewRequestDTO dto)
        {
            try
            {
                DoctorProfile? profile = await _context.DoctorProfiles.Include(d => d.User)
                                                                      .Include(d => d.Specialty)
                                                                      .FirstOrDefaultAsync(d => d.Id == dto.Id);

                if (profile is null)
                {
                    return Response.Failure($"No existe solicitud de doctor con id: {dto.Id}");
                }

                if (!dto.Approve && string.IsNullOrWhiteSpace(dto.Notes))
                {
                    return Response.Failure("Debe indicar el motivo del rechazo en las observaciones");
                }

                if (dto.Approve)
                {
                    if (profile.Specialty is null || profile.Specialty.Status != ApprovalStatus.Approved || !profile.Specialty.IsActive)
                    {
                        return Response.Failure("No se puede aprobar el doctor: su especialidad no está aprobada o está inactiva. Revise primero la especialidad.");
                    }

                    if (profile.User is null || !profile.User.IsActive)
                    {
                        return Response.Failure("No se puede aprobar el doctor: su cuenta está desactivada.");
                    }
                }

                User? reviewer = await _accountService.GetAccountUserAsync();

                profile.Status = dto.Approve ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
                profile.ReviewedBy = reviewer?.Email;
                profile.ReviewedAt = DateTime.UtcNow;
                profile.ReviewNotes = dto.Notes?.Trim();

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(dto.Approve ? AuditActions.APPROVE : AuditActions.REJECT,
                                             "Doctor",
                                             profile.Id.ToString(),
                                             $"Doctor '{profile.User?.Name}' {(dto.Approve ? "aprobado" : "rechazado")}");

                return Response.Success(dto.Approve ? "Doctor aprobado con éxito" : "Solicitud de doctor rechazada");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        public async Task<Response> ReviewSpecialtyAsync(ReviewRequestDTO dto)
        {
            try
            {
                Specialty? specialty = await _context.Specialties.FirstOrDefaultAsync(s => s.Id == dto.Id);

                if (specialty is null)
                {
                    return Response.Failure($"No existe especialidad con id: {dto.Id}");
                }

                if (!dto.Approve && string.IsNullOrWhiteSpace(dto.Notes))
                {
                    return Response.Failure("Debe indicar el motivo del rechazo en las observaciones");
                }

                if (!dto.Approve)
                {
                    bool inUse = await _context.DoctorProfiles.AnyAsync(d => d.SpecialtyId == specialty.Id && d.Status == ApprovalStatus.Approved);

                    if (inUse)
                    {
                        return Response.Failure("No se puede rechazar: hay doctores aprobados con esta especialidad.");
                    }
                }

                User? reviewer = await _accountService.GetAccountUserAsync();

                specialty.Status = dto.Approve ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
                specialty.ReviewedBy = reviewer?.Email;
                specialty.ReviewedAt = DateTime.UtcNow;
                specialty.ReviewNotes = dto.Notes?.Trim();

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(dto.Approve ? AuditActions.APPROVE : AuditActions.REJECT,
                                             "Especialidad",
                                             specialty.Id.ToString(),
                                             $"Especialidad '{specialty.Name}' {(dto.Approve ? "aprobada" : "rechazada")}");

                return Response.Success(dto.Approve ? "Especialidad aprobada con éxito" : "Especialidad rechazada");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        private static DoctorApprovalDTO ToDoctorDTO(DoctorProfile profile)
        {
            return new DoctorApprovalDTO
            {
                DoctorProfileId = profile.Id,
                Name = profile.User?.Name ?? string.Empty,
                Email = profile.User?.Email,
                UserIsActive = profile.User?.IsActive ?? false,
                SpecialtyName = profile.Specialty?.Name,
                SpecialtyStatus = profile.Specialty?.Status ?? ApprovalStatus.Pending,
                ProfessionalLicense = profile.ProfessionalLicense,
                Biography = profile.Biography,
                Status = profile.Status,
                RequestedBy = profile.RequestedBy,
                RequestedAt = profile.RequestedAt,
                ReviewedBy = profile.ReviewedBy,
                ReviewedAt = profile.ReviewedAt,
                ReviewNotes = profile.ReviewNotes
            };
        }

        private static SpecialtyApprovalDTO ToSpecialtyDTO(Specialty specialty)
        {
            return new SpecialtyApprovalDTO
            {
                Id = specialty.Id,
                Name = specialty.Name,
                Description = specialty.Description,
                Status = specialty.Status,
                RequestedBy = specialty.RequestedBy,
                RequestedAt = specialty.RequestedAt,
                ReviewedBy = specialty.ReviewedBy,
                ReviewedAt = specialty.ReviewedAt,
                ReviewNotes = specialty.ReviewNotes
            };
        }
    }
}

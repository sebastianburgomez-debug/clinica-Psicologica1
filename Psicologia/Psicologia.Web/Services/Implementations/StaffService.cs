using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.Staff;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Gestión de cuentas de doctores y secretarias (creación, edición y borrado lógico)
    public class StaffService : IStaffService
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IAccountService _accountService;
        private readonly IAuditService _auditService;

        public StaffService(DataContext context,
                            UserManager<User> userManager,
                            IAccountService accountService,
                            IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _accountService = accountService;
            _auditService = auditService;
        }

        public async Task<Response<PaginationResponse<StaffDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            try
            {
                IQueryable<User> query = _context.Users.AsNoTracking()
                                                       .Include(u => u.Role)
                                                       .Include(u => u.DoctorProfile)
                                                           .ThenInclude(d => d!.Specialty)
                                                       .Where(u => u.Role!.Name == Env.DOCTOR_ROLE_NAME
                                                                   || u.Role!.Name == Env.SECRETARY_ROLE_NAME);

                if (!string.IsNullOrWhiteSpace(request.Filter))
                {
                    string filter = request.Filter.Trim().ToLower();

                    query = query.Where(u => u.Name.ToLower().Contains(filter)
                                             || u.Email!.ToLower().Contains(filter)
                                             || u.Role!.Name.ToLower().Contains(filter)
                                             || (u.DoctorProfile != null && u.DoctorProfile.Specialty!.Name.ToLower().Contains(filter)));
                }

                query = query.OrderByDescending(u => u.IsActive)
                             .ThenBy(u => u.Name);

                PagedList<User> list = await PagedList<User>.ToPagedListAsync(query, request);

                List<StaffDTO> items = list.Select(ToStaffDTO).ToList();

                PaginationResponse<StaffDTO> dto = new PaginationResponse<StaffDTO>
                {
                    List = new PagedList<StaffDTO>(items, list.TotalCount, list.CurrentPage, list.RecordsPerPage),
                    CurrentPage = list.CurrentPage,
                    TotalPages = list.TotalPages,
                    RecordsPerPage = list.RecordsPerPage,
                    TotalCount = list.TotalCount,
                    Filter = request.Filter,
                };

                return Response<PaginationResponse<StaffDTO>>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<PaginationResponse<StaffDTO>>.Failure(ex);
            }
        }

        public async Task<Response<UpdateStaffDTO>> GetOneAsync(Guid id)
        {
            try
            {
                User? user = await GetStaffUserAsync(id.ToString());

                if (user is null)
                {
                    return Response<UpdateStaffDTO>.Failure($"No existe personal con id: '{id}'");
                }

                UpdateStaffDTO dto = new UpdateStaffDTO
                {
                    Id = id,
                    Name = user.Name,
                    Email = user.Email,
                    StaffType = user.Role!.Name == Env.DOCTOR_ROLE_NAME ? StaffType.Doctor : StaffType.Secretary,
                    SpecialtyId = user.DoctorProfile?.SpecialtyId,
                    ProfessionalLicense = user.DoctorProfile?.ProfessionalLicense,
                    Biography = user.DoctorProfile?.Biography,
                    DoctorStatus = user.DoctorProfile?.Status
                };

                return Response<UpdateStaffDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<UpdateStaffDTO>.Failure(ex);
            }
        }

        public async Task<Response<StaffDTO>> CreateAsync(CreateStaffDTO dto)
        {
            string email = dto.Email.Trim();
            string name = dto.Name.Trim();

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return Response<StaffDTO>.Failure($"Ya existe un usuario con el correo \"{email}\".");
            }

            string roleName = dto.StaffType == StaffType.Doctor ? Env.DOCTOR_ROLE_NAME : Env.SECRETARY_ROLE_NAME;

            Role? role = await _context.PsicologiaRoles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role is null)
            {
                return Response<StaffDTO>.Failure($"No existe el rol \"{roleName}\". Verifique los datos iniciales (seeders).");
            }

            if (dto.StaffType == StaffType.Doctor)
            {
                Response validation = await ValidateDoctorDataAsync(dto.SpecialtyId, dto.ProfessionalLicense, null);

                if (!validation.IsSuccess)
                {
                    return Response<StaffDTO>.Failure(validation.Message ?? "Datos del doctor inválidos");
                }
            }

            bool autoApprove = await _accountService.AccountUserIsAuthorizedAsync(Env.APPROVALS_MANAGE_PERMISSION);
            User? accountUser = await _accountService.GetAccountUserAsync();

            using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    User user = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = name,
                        Email = email,
                        UserName = email,
                        RoleId = role.Id
                    };

                    IdentityResult result = await _userManager.CreateAsync(user, dto.Password);

                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return Response<StaffDTO>.Failure("No se pudo crear la cuenta", result.Errors.Select(e => e.Description).ToList());
                    }

                    // TODO: Send token by email for confirmation
                    string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);

                    if (dto.StaffType == StaffType.Doctor)
                    {
                        DoctorProfile profile = new DoctorProfile
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            SpecialtyId = dto.SpecialtyId!.Value,
                            ProfessionalLicense = dto.ProfessionalLicense!.Trim(),
                            Biography = dto.Biography?.Trim(),
                            Status = autoApprove ? ApprovalStatus.Approved : ApprovalStatus.Pending,
                            RequestedBy = accountUser?.Email,
                            RequestedAt = DateTime.UtcNow,
                            ReviewedBy = autoApprove ? accountUser?.Email : null,
                            ReviewedAt = autoApprove ? DateTime.UtcNow : null
                        };

                        await _context.DoctorProfiles.AddAsync(profile);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    await _auditService.LogAsync(AuditActions.CREATE, "Personal", user.Id, $"Se creó la cuenta de {roleName} '{name}' ({email})");

                    string message = dto.StaffType == StaffType.Doctor && !autoApprove
                        ? "Doctor registrado. Queda pendiente de aprobación por la Dirección"
                        : "Cuenta creada con éxito";

                    return Response<StaffDTO>.Success(new StaffDTO
                    {
                        Id = Guid.Parse(user.Id),
                        Name = name,
                        Email = email,
                        RoleName = roleName,
                        IsActive = true,
                        IsDoctor = dto.StaffType == StaffType.Doctor
                    }, message);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Response<StaffDTO>.Failure(ex);
                }
            }
        }

        public async Task<Response<StaffDTO>> UpdateAsync(UpdateStaffDTO dto)
        {
            try
            {
                User? user = await GetStaffUserAsync(dto.Id.ToString(), tracking: true);

                if (user is null)
                {
                    return Response<StaffDTO>.Failure($"No existe personal con id: '{dto.Id}'");
                }

                bool isDoctor = user.Role!.Name == Env.DOCTOR_ROLE_NAME;

                user.Name = dto.Name.Trim();

                if (isDoctor)
                {
                    Response validation = await ValidateDoctorDataAsync(dto.SpecialtyId, dto.ProfessionalLicense, user.DoctorProfile?.Id);

                    if (!validation.IsSuccess)
                    {
                        return Response<StaffDTO>.Failure(validation.Message ?? "Datos del doctor inválidos");
                    }

                    if (user.DoctorProfile is null)
                    {
                        // Doctor creado desde el módulo Usuarios sin perfil profesional: se crea pendiente
                        User? accountUser = await _accountService.GetAccountUserAsync();

                        await _context.DoctorProfiles.AddAsync(new DoctorProfile
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            SpecialtyId = dto.SpecialtyId!.Value,
                            ProfessionalLicense = dto.ProfessionalLicense!.Trim(),
                            Biography = dto.Biography?.Trim(),
                            Status = ApprovalStatus.Pending,
                            RequestedBy = accountUser?.Email,
                            RequestedAt = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        user.DoctorProfile.SpecialtyId = dto.SpecialtyId!.Value;
                        user.DoctorProfile.ProfessionalLicense = dto.ProfessionalLicense!.Trim();
                        user.DoctorProfile.Biography = dto.Biography?.Trim();
                    }
                }

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Personal", user.Id, $"Se actualizó la cuenta de {user.Role.Name} '{user.Name}' ({user.Email})");

                return Response<StaffDTO>.Success(ToStaffDTO(user), "Cuenta actualizada con éxito");
            }
            catch (Exception ex)
            {
                return Response<StaffDTO>.Failure(ex);
            }
        }

        public async Task<Response> ToggleAsync(ToggleStaffStatusDTO dto)
        {
            try
            {
                User? user = await GetStaffUserAsync(dto.UserId.ToString(), tracking: true);

                if (user is null)
                {
                    return Response.Failure($"No existe personal con id: '{dto.UserId}'");
                }

                User? accountUser = await _accountService.GetAccountUserAsync();

                if (accountUser?.Id == user.Id)
                {
                    return Response.Failure("No puede desactivar su propia cuenta");
                }

                user.IsActive = !dto.Deactivate;

                // Actualizar el security stamp invalida las sesiones abiertas del usuario
                IdentityResult result = await _userManager.UpdateSecurityStampAsync(user);

                if (!result.Succeeded)
                {
                    return Response.Failure("No se pudo actualizar el estado de la cuenta", result.Errors.Select(e => e.Description).ToList());
                }

                await _auditService.LogAsync(dto.Deactivate ? AuditActions.DEACTIVATE : AuditActions.ACTIVATE,
                                             "Personal",
                                             user.Id,
                                             $"Cuenta de {user.Role!.Name} '{user.Name}' {(dto.Deactivate ? "desactivada" : "activada")}");

                string message = dto.Deactivate ? "Cuenta desactivada con éxito" : "Cuenta activada con éxito";

                // Informativo: citas futuras que quedan a cargo de un doctor desactivado
                if (dto.Deactivate && user.DoctorProfile is not null)
                {
                    int futureAppointments = await _context.Appointments.CountAsync(a => a.DoctorProfileId == user.DoctorProfile.Id
                                                                                         && a.Status == AppointmentStatus.Scheduled
                                                                                         && a.StartsAt > DateTime.Now);
                    if (futureAppointments > 0)
                    {
                        message += $". Atención: el doctor tiene {futureAppointments} cita(s) programada(s) por reasignar o cancelar";
                    }
                }

                return Response.Success(message);
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        private async Task<User?> GetStaffUserAsync(string id, bool tracking = false)
        {
            IQueryable<User> query = _context.Users.Include(u => u.Role)
                                                   .Include(u => u.DoctorProfile)
                                                       .ThenInclude(d => d!.Specialty);

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(u => u.Id == id
                                                        && (u.Role!.Name == Env.DOCTOR_ROLE_NAME || u.Role!.Name == Env.SECRETARY_ROLE_NAME));
        }

        private async Task<Response> ValidateDoctorDataAsync(Guid? specialtyId, string? professionalLicense, Guid? currentProfileId)
        {
            if (specialtyId is null || string.IsNullOrWhiteSpace(professionalLicense))
            {
                return Response.Failure("La especialidad y la tarjeta profesional son requeridas para los doctores");
            }

            bool specialtyIsValid = await _context.Specialties.AnyAsync(s => s.Id == specialtyId
                                                                             && s.IsActive
                                                                             && s.Status == ApprovalStatus.Approved);
            if (!specialtyIsValid)
            {
                return Response.Failure("La especialidad seleccionada no existe, no está activa o no ha sido aprobada");
            }

            string license = professionalLicense.Trim();

            bool licenseInUse = await _context.DoctorProfiles.AnyAsync(d => d.ProfessionalLicense == license
                                                                            && d.Id != currentProfileId);
            if (licenseInUse)
            {
                return Response.Failure($"La tarjeta profesional \"{license}\" ya está registrada");
            }

            return Response.Success();
        }

        private static StaffDTO ToStaffDTO(User user)
        {
            return new StaffDTO
            {
                Id = Guid.Parse(user.Id),
                Name = user.Name,
                Email = user.Email,
                RoleName = user.Role?.Name,
                IsActive = user.IsActive,
                IsDoctor = user.Role?.Name == Env.DOCTOR_ROLE_NAME,
                SpecialtyName = user.DoctorProfile?.Specialty?.Name,
                ProfessionalLicense = user.DoctorProfile?.ProfessionalLicense,
                DoctorStatus = user.DoctorProfile?.Status
            };
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.Account;
using Psicologia.Web.DTOs.User;
using Psicologia.Web.Services.Abstractions;
using System.Security.Claims;

namespace Psicologia.Web.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUsersService _usersService;
        private readonly IHttpContextAccessor _httpContextAccesor;
        private readonly DataContext _context;
        private readonly IAuditService _auditService;

        public AccountService(UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              IUsersService usersService,
                              IHttpContextAccessor httpContextAccesor,
                              DataContext context,
                              IAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _usersService = usersService;
            _httpContextAccesor = httpContextAccesor;
            _context = context;
            _auditService = auditService;
        }

        public bool AccountUserIsAuthenticated()
        {
            ClaimsPrincipal? user = _httpContextAccesor.HttpContext?.User;
            return user?.Identity is not null && user.Identity.IsAuthenticated;
        }

        public async Task<bool> AccountUserIsAuthorizedAsync(string code)
        {
            User? user = await GetAccountUserAsync();

            if (user is null)
            {
                return false;
            }

            if (user.Role?.Name == Env.SUPER_ADMIN_ROLE_NAME)
            {
                return true;
            }

            return await _context.Permissions.AnyAsync(p => p.Code == code
                                                         && p.RolePermissions!.Any(rp => rp.RoleId == user.RoleId));
        }

        public async Task<bool> AccountUserIsSuperAdminAsync()
        {
            User? user = await GetAccountUserAsync();

            if (user is null)
            {
                return false;
            }

            return user.Role?.Name == Env.SUPER_ADMIN_ROLE_NAME;
        }

        public async Task<Response<IdentityResult>> ConfirmUserAsync(User user, string token)
        {
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);

            return new Response<IdentityResult>
            {
                Result = result,
                IsSuccess = result.Succeeded
            };
        }

        public async Task<Response<string>> GenerateConfirmationTokenAsync(User user)
        {
            string result = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            return Response<string>.Success(result);
        }

        public async Task<Response<SignInResult>> LoginAsync(LoginDTO dto)
        {
            User? user = await _userManager.FindByEmailAsync(dto.Email);

            // Cuenta desactivada (borrado lógico): no se permite el ingreso
            if (user is not null && !user.IsActive)
            {
                bool validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);

                await _auditService.LogAsync(AuditActions.LOGIN_FAILED, "Cuenta", user.Id, "Intento de ingreso con una cuenta desactivada", dto.Email);

                return new Response<SignInResult>
                {
                    Result = SignInResult.NotAllowed,
                    IsSuccess = false,
                    // Solo se revela que la cuenta está desactivada si la contraseña es correcta
                    Message = validPassword ? "Su cuenta está desactivada. Comuníquese con el administrador." : null
                };
            }

            SignInResult result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, false, false);

            await _auditService.LogAsync(result.Succeeded ? AuditActions.LOGIN : AuditActions.LOGIN_FAILED,
                                         "Cuenta",
                                         user?.Id,
                                         result.Succeeded ? "Inicio de sesión exitoso" : "Credenciales inválidas",
                                         dto.Email);

            return new Response<SignInResult>
            {
                Result = result,
                IsSuccess = result.Succeeded
            };
        }

        public async Task LogoutAsync()
        {
            string? userName = _httpContextAccesor.HttpContext?.User?.Identity?.Name;

            await _signInManager.SignOutAsync();

            await _auditService.LogAsync(AuditActions.LOGOUT, "Cuenta", null, "Cierre de sesión", userName);
        }

        public async Task<Response<AccountUserDTO>> UpdateAccountUserAsync(AccountUserDTO dto)
        {
            try
            {
                User? accountUser = await GetAccountUserAsync();

                if (accountUser is null || accountUser.Id != dto.Id.ToString())
                {
                    return Response<AccountUserDTO>.Failure("No es posible actualizar el usuario");
                }

                // RoleId vacío: el usuario no puede cambiar su propio rol
                UpdateUserDTO userDto = new UpdateUserDTO
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    RoleId = Guid.Empty
                };

                Response<UserDTO> userDtoResponse = await _usersService.UpdateUserAsync(userDto);

                if (userDtoResponse.IsSuccess)
                {
                    return Response<AccountUserDTO>.Success(dto, "Usuario actualizado con éxito");
                }

                return Response<AccountUserDTO>.Failure(userDtoResponse.Message ?? "Ha ocurrido un error", userDtoResponse.Errors);
            }
            catch (Exception ex)
            {
                return Response<AccountUserDTO>.Failure(ex);
            }
        }

        public async Task<User?> GetAccountUserAsync()
        {
            ClaimsPrincipal? claimUser = _httpContextAccesor.HttpContext?.User;

            // Check identity (session)
            if (claimUser?.Identity is null || !claimUser.Identity.IsAuthenticated || string.IsNullOrEmpty(claimUser.Identity.Name))
            {
                return null;
            }

            Response<User> response = await _usersService.GetUserByEmailAsync(claimUser.Identity.Name);

            if (!response.IsSuccess || response.Result is null)
            {
                return null;
            }

            // Una cuenta desactivada con una cookie vigente no tiene acceso a nada
            if (!response.Result.IsActive)
            {
                return null;
            }

            return response.Result;
        }

        public async Task<DoctorProfile?> GetAccountDoctorProfileAsync()
        {
            User? user = await GetAccountUserAsync();

            if (user is null)
            {
                return null;
            }

            return await _context.DoctorProfiles.Include(d => d.Specialty)
                                                .Include(d => d.User)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(d => d.UserId == user.Id);
        }
    }
}

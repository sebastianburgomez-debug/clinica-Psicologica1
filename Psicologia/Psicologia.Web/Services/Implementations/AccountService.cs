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

        public AccountService(UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              IUsersService usersService,
                              IHttpContextAccessor httpContextAccesor,
                              DataContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _usersService = usersService;
            _httpContextAccesor = httpContextAccesor;
            _context = context;
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
            SignInResult result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, false, false);

            return new Response<SignInResult>
            {
                Result = result,
                IsSuccess = result.Succeeded
            };
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
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

        private async Task<User?> GetAccountUserAsync()
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

            return response.Result;
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Psicologia.Web.Core;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.Account;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IAccountService
    {
        public Task<Response<IdentityResult>> ConfirmUserAsync(User user, string token);
        public Task<Response<string>> GenerateConfirmationTokenAsync(User user);
        public Task<Response<SignInResult>> LoginAsync(LoginDTO dto);
        public Task LogoutAsync();
        public Task<Response<AccountUserDTO>> UpdateAccountUserAsync(AccountUserDTO dto);
        public bool AccountUserIsAuthenticated();
        public Task<bool> AccountUserIsAuthorizedAsync(string code);
        public Task<bool> AccountUserIsSuperAdminAsync();

        // Usuario de la sesión actual (null si no hay sesión o si la cuenta está inactiva)
        public Task<User?> GetAccountUserAsync();

        // Perfil de doctor del usuario de la sesión (null si no es doctor)
        public Task<DoctorProfile?> GetAccountDoctorProfileAsync();
    }
}

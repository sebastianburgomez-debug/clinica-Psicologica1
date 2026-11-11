
using Psicologia.Data.Entities;

namespace Psicologia.Services.Abstractions
{
    public interface IAuthService
    {
        Task<Usuario?> AuthenticateAsync(string correo, string password);
    }
}

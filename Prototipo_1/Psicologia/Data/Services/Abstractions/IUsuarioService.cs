
using Psicologia.Data.Entities;

namespace Psicologia.Services.Abstractions
{
    public interface IUsuarioService
    {
        Task<bool> CrearUsuarioAsync(
            string nombre,
            string correo,
            string password,
            int roleId);

        Task<bool> ConfigurarPasswordInicialAdministradorAsync(
            string correo,
            string password);
    }
}

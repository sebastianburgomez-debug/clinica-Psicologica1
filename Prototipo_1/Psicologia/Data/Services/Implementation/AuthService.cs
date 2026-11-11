
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Psicologia.Data;
using Psicologia.Data.Entities;
using Psicologia.Services.Abstractions;

namespace Psicologia.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public AuthService(DataContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        public async Task<Usuario?> AuthenticateAsync(
            string correo,
            string password)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null || string.IsNullOrEmpty(usuario.PasswordHash))
            {
                return null;
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.PasswordHash,
                password
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return usuario;
        }
    }
}

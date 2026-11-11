
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Psicologia.Data;
using Psicologia.Data.Entities;
using Psicologia.Services.Abstractions;

namespace Psicologia.Services.Implementations
{
    public class UsuarioService : IUsuarioService
    {
        private readonly DataContext _context;
        private readonly PasswordHasher<Usuario> _hasher;

        public UsuarioService(DataContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<Usuario>();
        }

        public async Task<bool> ConfigurarPasswordInicialAdministradorAsync(
            string correo,
            string password)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null ||
                usuario.Role?.Name != "Administrador" ||
                !string.IsNullOrEmpty(usuario.PasswordHash))
            {
                return false;
            }

            usuario.PasswordHash = _hasher.HashPassword(usuario, password);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CrearUsuarioAsync(
            string nombre,
            string correo,
            string password,
            int roleId)
        {
            correo = correo.Trim().ToLowerInvariant();

            bool existe = await _context.Usuarios
                .AnyAsync(u => u.Correo.ToLower() == correo);

            if (existe)
                return false;

            bool rolExiste = await _context.Roles
                .AnyAsync(r => r.RoleId == roleId);

            if (!rolExiste)
                throw new InvalidOperationException(
                    "El rol seleccionado no existe.");

            var usuario = new Usuario
            {
                Name = nombre.Trim(),
                Correo = correo,
                RoleId = roleId
            };

            usuario.PasswordHash = _hasher.HashPassword(usuario, password);

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}

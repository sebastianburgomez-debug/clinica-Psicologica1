using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Data.Entities;

namespace Psicologia.Web.Data.Seeders
{
    public class PermissionsSeeder
    {
        private readonly DataContext _context;

        public PermissionsSeeder(DataContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            List<Permission> permissions = [ ..Specialties(), ..Users(), ..Roles() ];

            foreach (Permission permission in permissions)
            {
                bool exists = await _context.Permissions.AnyAsync(p => p.Code == permission.Code);

                if (!exists)
                {
                    await _context.Permissions.AddAsync(permission);
                }
            }

            await _context.SaveChangesAsync();
        }

        private List<Permission> Specialties()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "specialties.show", Name = "Ver Especialidades", Module = "Especialidades"},
                new Permission { Id = Guid.NewGuid(), Code = "specialties.create", Name = "Crear Especialidades", Module = "Especialidades"},
                new Permission { Id = Guid.NewGuid(), Code = "specialties.update", Name = "Editar Especialidades", Module = "Especialidades"},
                new Permission { Id = Guid.NewGuid(), Code = "specialties.delete", Name = "Eliminar Especialidades", Module = "Especialidades"},
            };
        }

        private List<Permission> Users()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "users.show", Name = "Ver Usuarios", Module = "Usuarios"},
                new Permission { Id = Guid.NewGuid(), Code = "users.create", Name = "Crear Usuarios", Module = "Usuarios"},
                new Permission { Id = Guid.NewGuid(), Code = "users.update", Name = "Editar Usuarios", Module = "Usuarios"},
                new Permission { Id = Guid.NewGuid(), Code = "users.delete", Name = "Eliminar Usuarios", Module = "Usuarios"},
            };
        }

        private List<Permission> Roles()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "roles.show", Name = "Ver Roles", Module = "Roles"},
                new Permission { Id = Guid.NewGuid(), Code = "roles.create", Name = "Crear Roles", Module = "Roles"},
                new Permission { Id = Guid.NewGuid(), Code = "roles.update", Name = "Editar Roles", Module = "Roles"},
                new Permission { Id = Guid.NewGuid(), Code = "roles.delete", Name = "Eliminar Roles", Module = "Roles"},
            };
        }
    }
}

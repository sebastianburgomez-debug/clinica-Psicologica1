using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Data.Entities;

namespace Psicologia.Web.Data.Seeders
{//poblan datos iniciales
    public class RolesSeeder
    {
        private readonly DataContext _context;

        public RolesSeeder(DataContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            List<Role> roles = new List<Role>
            {
                new Role { Id = Guid.NewGuid(), Name = "Administrador" },
                new Role { Id = Guid.NewGuid(), Name = "Doctor" },
                new Role { Id = Guid.NewGuid(), Name = "Paciente" },
            };

            foreach (Role role in roles)
            {
                bool exists = await _context.PsicologiaRoles.AnyAsync(r => r.Name == role.Name);

                if (!exists)
                {
                    await _context.PsicologiaRoles.AddAsync(role);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

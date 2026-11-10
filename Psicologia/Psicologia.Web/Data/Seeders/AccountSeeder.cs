using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Data.Seeders
{
    public class AccountSeeder
    {
        private readonly IUsersService _usersService;
        private readonly IAccountService _accountService;
        private readonly DataContext _context;

        private const string DOCTOR_ROLE_NAME = "Doctor";
        private const string PATIENT_ROLE_NAME = "Paciente";

        public AccountSeeder(IUsersService usersService, DataContext context, IAccountService accountService)
        {
            _usersService = usersService;
            _context = context;
            _accountService = accountService;
        }

        public async Task SeedAsync()
        {
            await CheckRolesAsync();
            await CheckUsersAsync();
        }

        private async Task CheckRolesAsync()
        {
            await AdminRoleAsync();
            await DoctorRoleAsync();
            await PatientRoleAsync();
        }

        private async Task AdminRoleAsync()
        {
            bool exists = await _context.PsicologiaRoles.AnyAsync(r => r.Name == Env.SUPER_ADMIN_ROLE_NAME);

            if (!exists)
            {
                Role role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = Env.SUPER_ADMIN_ROLE_NAME
                };

                await _context.PsicologiaRoles.AddAsync(role);
                await _context.SaveChangesAsync();
            }
        }

        private async Task DoctorRoleAsync()
        {
            Role? role = await _context.PsicologiaRoles.FirstOrDefaultAsync(r => r.Name == DOCTOR_ROLE_NAME);

            if (role is null)
            {
                role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = DOCTOR_ROLE_NAME
                };

                await _context.PsicologiaRoles.AddAsync(role);
                await _context.SaveChangesAsync();
            }

            // El doctor solo tiene permisos iniciales si aún no se le ha asignado ninguno
            bool hasPermissions = await _context.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id);

            if (!hasPermissions)
            {
                List<Permission> permissions = await _context.Permissions.Where(p => p.Module == "Especialidades")
                                                                         .ToListAsync();

                foreach (Permission permission in permissions)
                {
                    await _context.RolePermissions.AddAsync(new RolePermission { PermissionId = permission.Id, RoleId = role.Id });
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task PatientRoleAsync()
        {
            bool exists = await _context.PsicologiaRoles.AnyAsync(r => r.Name == PATIENT_ROLE_NAME);

            if (!exists)
            {
                Role role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = PATIENT_ROLE_NAME
                };

                await _context.PsicologiaRoles.AddAsync(role);
                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckUsersAsync()
        {
            await AdminUserAsync();
            await DoctorUserAsync();
            await PatientUserAsync();
        }

        private async Task AdminUserAsync()
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "admin@psicologia.com");

            if (user is null)
            {
                Role adminRole = await _context.PsicologiaRoles.FirstAsync(r => r.Name == Env.SUPER_ADMIN_ROLE_NAME);

                user = new User
                {
                    Email = "admin@psicologia.com",
                    UserName = "admin@psicologia.com",
                    Name = "Administrador",
                    RoleId = adminRole.Id,
                };

                await _usersService.AddUserAsync(user, "1234");

                string token = (await _accountService.GenerateConfirmationTokenAsync(user)).Result!;

                await _accountService.ConfirmUserAsync(user, token);
            }
        }

        private async Task DoctorUserAsync()
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "doctor@psicologia.com");

            if (user is null)
            {
                Role role = await _context.PsicologiaRoles.FirstAsync(r => r.Name == DOCTOR_ROLE_NAME);

                user = new User
                {
                    Email = "doctor@psicologia.com",
                    UserName = "doctor@psicologia.com",
                    Name = "Juan Pérez",
                    RoleId = role.Id,
                };

                await _usersService.AddUserAsync(user, "1234");

                string token = (await _accountService.GenerateConfirmationTokenAsync(user)).Result!;

                await _accountService.ConfirmUserAsync(user, token);
            }
        }

        private async Task PatientUserAsync()
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "paciente@psicologia.com");

            if (user is null)
            {
                Role role = await _context.PsicologiaRoles.FirstAsync(r => r.Name == PATIENT_ROLE_NAME);

                user = new User
                {
                    Email = "paciente@psicologia.com",
                    UserName = "paciente@psicologia.com",
                    Name = "Ana Gómez",
                    RoleId = role.Id,
                };

                await _usersService.AddUserAsync(user, "1234");

                string token = (await _accountService.GenerateConfirmationTokenAsync(user)).Result!;

                await _accountService.ConfirmUserAsync(user, token);
            }
        }
    }
}

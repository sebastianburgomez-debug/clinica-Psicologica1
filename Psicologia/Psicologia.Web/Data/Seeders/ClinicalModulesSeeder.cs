using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Data.Seeders
{
    // Datos iniciales de los módulos Panel de Dirección, Atención Clínica y Administración del Sistema.
    // Todos los pasos son idempotentes: solo insertan lo que falta.
    public class ClinicalModulesSeeder
    {
        private readonly DataContext _context;
        private readonly IUsersService _usersService;
        private readonly IAccountService _accountService;

        // Permisos por defecto de cada rol (solo se asignan si el rol aún no tiene ninguno de ellos)
        private static readonly string[] DOCTOR_PERMISSIONS =
        [
            "agenda.show", "agenda.manage",
            "appointments.show", "appointments.update",
            "clinicalrecords.show", "clinicalrecords.create", "clinicalrecords.update",
            "doctorpanel.show"
        ];

        private static readonly string[] SECRETARY_PERMISSIONS =
        [
            "agenda.show",
            "appointments.show", "appointments.create", "appointments.update"
        ];

        private static readonly string[] PATIENT_PERMISSIONS =
        [
            "appointments.show", "appointments.create", "appointments.update"
        ];

        public ClinicalModulesSeeder(DataContext context, IUsersService usersService, IAccountService accountService)
        {
            _context = context;
            _usersService = usersService;
            _accountService = accountService;
        }

        public async Task SeedAsync()
        {
            await BusinessHoursAsync();
            await ClinicalServicesAsync();
            await RolePermissionsAsync(Env.DOCTOR_ROLE_NAME, DOCTOR_PERMISSIONS);
            await RolePermissionsAsync(Env.SECRETARY_ROLE_NAME, SECRETARY_PERMISSIONS);
            await RolePermissionsAsync(Env.PATIENT_ROLE_NAME, PATIENT_PERMISSIONS);
            await SecretaryUserAsync();
            await DoctorProfileAsync();
        }

        private async Task BusinessHoursAsync()
        {
            if (await _context.BusinessHours.AnyAsync())
            {
                return;
            }

            List<BusinessHour> hours =
            [
                new BusinessHour { Id = Guid.NewGuid(), DayOfWeek = DayOfWeek.Monday, IsOpen = true, OpenTime = new TimeOnly(8, 0), CloseTime = new TimeOnly(18, 0) },
                new BusinessHour { Id = Guid.NewGuid(), DayOfWeek = DayOfWeek.Tuesday, IsOpen = true, OpenTime = new TimeOnly(8, 0), CloseTime = new TimeOnly(18, 0) },
                new BusinessHour { Id = Guid.NewGuid(), DayOfWeek = DayOfWeek.Wednesday, IsOpen = true, OpenTime = new TimeOnly(8, 0), CloseTime = new TimeOnly(18, 0) },
                new BusinessHour { Id = Guid.NewGuid(), DayOfWeek = DayOfWeek.Thursday, IsOpen = true, OpenTime = new TimeOnly(8, 0), CloseTime = new TimeOnly(18, 0) },
                new BusinessHour { Id = Guid.NewGuid(), DayOfWeek = DayOfWeek.Friday, IsOpen = true, OpenTime = new TimeOnly(8, 0), CloseTime = new TimeOnly(18, 0) },
                new BusinessHour { Id = Guid.NewGuid(), DayOfWeek = DayOfWeek.Saturday, IsOpen = true, OpenTime = new TimeOnly(8, 0), CloseTime = new TimeOnly(12, 0) },
                new BusinessHour { Id = Guid.NewGuid(), DayOfWeek = DayOfWeek.Sunday, IsOpen = false, OpenTime = new TimeOnly(8, 0), CloseTime = new TimeOnly(12, 0) },
            ];

            await _context.BusinessHours.AddRangeAsync(hours);
            await _context.SaveChangesAsync();
        }

        private async Task ClinicalServicesAsync()
        {
            if (await _context.ClinicalServices.AnyAsync())
            {
                return;
            }

            Guid? clinical = await _context.Specialties.Where(s => s.Name == "Psicología Clínica")
                                                       .Select(s => (Guid?)s.Id)
                                                       .FirstOrDefaultAsync();

            Guid? neuro = await _context.Specialties.Where(s => s.Name == "Neuropsicología")
                                                    .Select(s => (Guid?)s.Id)
                                                    .FirstOrDefaultAsync();

            // Tarifas de referencia meramente informativas
            List<ClinicalService> services =
            [
                new ClinicalService { Id = Guid.NewGuid(), Name = "Consulta de orientación inicial", Description = "Primera entrevista y orientación del caso", DurationMinutes = 30, ReferenceRate = 80000, SpecialtyId = null },
                new ClinicalService { Id = Guid.NewGuid(), Name = "Psicoterapia individual", Description = "Sesión de psicoterapia individual", DurationMinutes = 50, ReferenceRate = 150000, SpecialtyId = clinical },
                new ClinicalService { Id = Guid.NewGuid(), Name = "Terapia de pareja", Description = "Sesión de terapia de pareja", DurationMinutes = 60, ReferenceRate = 200000, SpecialtyId = clinical },
                new ClinicalService { Id = Guid.NewGuid(), Name = "Evaluación neuropsicológica", Description = "Aplicación e interpretación de pruebas", DurationMinutes = 90, ReferenceRate = 350000, SpecialtyId = neuro },
            ];

            await _context.ClinicalServices.AddRangeAsync(services);
            await _context.SaveChangesAsync();
        }

        private async Task RolePermissionsAsync(string roleName, string[] codes)
        {
            Role? role = await _context.PsicologiaRoles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role is null)
            {
                return;
            }

            List<string> currentCodes = await _context.RolePermissions.Where(rp => rp.RoleId == role.Id)
                                                                      .Select(rp => rp.Permission!.Code)
                                                                      .ToListAsync();

            // Si el administrador ya asignó alguno de estos permisos, se respeta su configuración
            if (currentCodes.Any(c => codes.Contains(c)))
            {
                return;
            }

            List<Permission> permissions = await _context.Permissions.ToListAsync();

            foreach (Permission permission in permissions.Where(p => codes.Contains(p.Code)))
            {
                await _context.RolePermissions.AddAsync(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
            }

            await _context.SaveChangesAsync();
        }

        private async Task SecretaryUserAsync()
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "secretaria@psicologia.com");

            if (user is not null)
            {
                return;
            }

            Role? role = await _context.PsicologiaRoles.FirstOrDefaultAsync(r => r.Name == Env.SECRETARY_ROLE_NAME);

            if (role is null)
            {
                return;
            }

            user = new User
            {
                Email = "secretaria@psicologia.com",
                UserName = "secretaria@psicologia.com",
                Name = "María Rodríguez",
                RoleId = role.Id,
            };

            await _usersService.AddUserAsync(user, "1234");

            string token = (await _accountService.GenerateConfirmationTokenAsync(user)).Result!;

            await _accountService.ConfirmUserAsync(user, token);
        }

        private async Task DoctorProfileAsync()
        {
            User? doctor = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "doctor@psicologia.com");

            if (doctor is null)
            {
                return;
            }

            DoctorProfile? profile = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == doctor.Id);

            if (profile is null)
            {
                Specialty? specialty = await _context.Specialties.FirstOrDefaultAsync(s => s.Name == "Psicología Clínica")
                                       ?? await _context.Specialties.FirstOrDefaultAsync(s => s.Status == ApprovalStatus.Approved);

                if (specialty is null)
                {
                    return;
                }

                profile = new DoctorProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = doctor.Id,
                    SpecialtyId = specialty.Id,
                    ProfessionalLicense = "TP-000001",
                    Biography = "Psicólogo clínico (usuario de demostración)",
                    Status = ApprovalStatus.Approved,
                    RequestedBy = "Sistema",
                    RequestedAt = DateTime.UtcNow,
                    ReviewedBy = "Sistema",
                    ReviewedAt = DateTime.UtcNow
                };

                await _context.DoctorProfiles.AddAsync(profile);
                await _context.SaveChangesAsync();
            }

            if (await _context.DoctorAvailabilities.AnyAsync(a => a.DoctorProfileId == profile.Id))
            {
                return;
            }

            // Lunes a viernes: 08:00 - 12:00 y 14:00 - 18:00 (dentro del horario de atención inicial)
            DayOfWeek[] days = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

            foreach (DayOfWeek day in days)
            {
                await _context.DoctorAvailabilities.AddAsync(new DoctorAvailability { Id = Guid.NewGuid(), DoctorProfileId = profile.Id, DayOfWeek = day, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(12, 0) });
                await _context.DoctorAvailabilities.AddAsync(new DoctorAvailability { Id = Guid.NewGuid(), DoctorProfileId = profile.Id, DayOfWeek = day, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(18, 0) });
            }

            await _context.SaveChangesAsync();
        }
    }
}

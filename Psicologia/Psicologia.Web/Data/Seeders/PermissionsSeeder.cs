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
            List<Permission> permissions =
            [
                ..Specialties(), ..Users(), ..Roles(),

                // Panel de Dirección
                ..Dashboard(), ..Approvals(), ..Policies(),

                // Atención Clínica
                ..Agenda(), ..Appointments(), ..ClinicalRecords(), ..DoctorPanel(),

                // Administración del Sistema
                ..Staff(), ..ClinicalServices(), ..Audit(),
                // Portal del paciente, reseñas y lista de espera
                ..Portal(), ..Reviews(), ..Waitlist()
            ];

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

        private List<Permission> Dashboard()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "dashboard.show", Name = "Ver Panel de Indicadores", Module = "Panel de Dirección"},
            };
        }

        private List<Permission> Approvals()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "approvals.show", Name = "Ver Solicitudes de Aprobación", Module = "Aprobaciones"},
                new Permission { Id = Guid.NewGuid(), Code = "approvals.manage", Name = "Aprobar / Rechazar Solicitudes", Module = "Aprobaciones"},
            };
        }

        private List<Permission> Policies()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "policies.show", Name = "Ver Políticas", Module = "Políticas"},
                new Permission { Id = Guid.NewGuid(), Code = "policies.update", Name = "Editar Horarios y Tarifas de Referencia", Module = "Políticas"},
            };
        }

        private List<Permission> Agenda()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "agenda.show", Name = "Ver Agenda", Module = "Agenda"},
                new Permission { Id = Guid.NewGuid(), Code = "agenda.manage", Name = "Gestionar Disponibilidad", Module = "Agenda"},
            };
        }

        private List<Permission> Appointments()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "appointments.show", Name = "Ver Citas", Module = "Citas"},
                new Permission { Id = Guid.NewGuid(), Code = "appointments.create", Name = "Agendar Citas", Module = "Citas"},
                new Permission { Id = Guid.NewGuid(), Code = "appointments.update", Name = "Cambiar Estado de Citas", Module = "Citas"},
            };
        }

        private List<Permission> ClinicalRecords()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "clinicalrecords.show", Name = "Ver Historias Clínicas", Module = "Historia Clínica"},
                new Permission { Id = Guid.NewGuid(), Code = "clinicalrecords.create", Name = "Abrir Historias Clínicas", Module = "Historia Clínica"},
                new Permission { Id = Guid.NewGuid(), Code = "clinicalrecords.update", Name = "Registrar Notas y Diagnósticos", Module = "Historia Clínica"},
            };
        }

        private List<Permission> DoctorPanel()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "doctorpanel.show", Name = "Ver Panel del Doctor", Module = "Panel del Doctor"},
            };
        }

        private List<Permission> Staff()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "staff.show", Name = "Ver Personal", Module = "Personal"},
                new Permission { Id = Guid.NewGuid(), Code = "staff.create", Name = "Crear Doctores y Secretarias", Module = "Personal"},
                new Permission { Id = Guid.NewGuid(), Code = "staff.update", Name = "Editar Personal", Module = "Personal"},
                new Permission { Id = Guid.NewGuid(), Code = "staff.toggle", Name = "Activar / Desactivar Personal", Module = "Personal"},
            };
        }

        private List<Permission> ClinicalServices()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "services.show", Name = "Ver Servicios", Module = "Servicios"},
                new Permission { Id = Guid.NewGuid(), Code = "services.create", Name = "Crear Servicios", Module = "Servicios"},
                new Permission { Id = Guid.NewGuid(), Code = "services.update", Name = "Editar Servicios", Module = "Servicios"},
                new Permission { Id = Guid.NewGuid(), Code = "services.delete", Name = "Eliminar Servicios", Module = "Servicios"},
            };
        }

        private List<Permission> Portal()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "portal.show", Name = "Ver Portal del Paciente", Module = "Portal del Paciente"},
                new Permission { Id = Guid.NewGuid(), Code = "portal.manage", Name = "Solicitar, Reprogramar y Cancelar Citas Propias", Module = "Portal del Paciente"},
            };
        }

        private List<Permission> Reviews()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "reviews.show", Name = "Ver Reseñas", Module = "Reseñas"},
                new Permission { Id = Guid.NewGuid(), Code = "reviews.create", Name = "Calificar Sesiones Propias", Module = "Reseñas"},
            };
        }

        private List<Permission> Waitlist()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "waitlist.show", Name = "Ver Lista de Espera", Module = "Lista de Espera"},
                new Permission { Id = Guid.NewGuid(), Code = "waitlist.manage", Name = "Gestionar Lista de Espera", Module = "Lista de Espera"},
            };
        }

        private List<Permission> Audit()
        {
            return new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "audit.show", Name = "Ver Auditoría", Module = "Auditoría"},
            };
        }
    }
}

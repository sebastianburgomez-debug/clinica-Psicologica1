# Clínica Psicológica · Módulos: Panel de Dirección, Atención Clínica y Administración del Sistema

Código para integrar en el proyecto **Psicologia.Web** (ASP.NET Core MVC · .NET 10 · EF Core 10 · Identity).
Sigue la misma arquitectura de PrivateBlog que ya usa el proyecto:
`Entidad → DTO → Servicio (Abstractions/Implementations, Response<T>) → Controlador con [CustomAuthorize] → Vista NiceAdmin/Bootstrap con Notyf y SweetAlert`.

> **Importante:** el código se escribió y revisó sin compilarlo (en el entorno donde se generó no había un SDK de .NET disponible).
> Compila la solución después de copiar los archivos; si aparece algún error menor, corrígelo antes de crear la migración.

---

## 1. Qué incluye

| Módulo | Funcionalidad | Controlador / ruta |
|---|---|---|
| **Panel de Dirección** | Panel de indicadores informativo: citas de hoy/periodo, estados, ocupación, gráficas, ocupación por doctor | `DashboardController` → `/Dashboard` |
| | Aprobación de nuevos doctores y especialidades (aprobar/rechazar con observaciones + historial) | `ApprovalsController` → `/Approvals` |
| | Políticas: horario de atención de la clínica y tarifas de referencia (solo informativas) | `PoliciesController` → `/Policies` |
| **Atención Clínica** | Agenda personal: bloques semanales de disponibilidad por doctor + vista semanal | `AgendaController` → `/Agenda` |
| | Historia clínica: datos generales, notas de sesión/evolución y diagnósticos | `ClinicalRecordsController` → `/ClinicalRecords` |
| | Panel informativo del doctor: atenciones, ocupación, servicios y tarifas de referencia | `DoctorPanelController` → `/DoctorPanel` |
| | Citas (módulo de soporte, ver nota abajo) | `AppointmentsController` → `/Appointments` |
| **Administración del Sistema** | Personal: crear, editar y activar/desactivar (borrado lógico) doctores y secretarias | `StaffController` → `/Staff` |
| | Catálogo de servicios psicológicos (CRUD + activar/desactivar) | `ClinicalServicesController` → `/ClinicalServices` |
| | Especialidades: se amplió el CRUD existente (descripción, estado y aprobación) | `SpecialtiesController` (existente) |
| | Auditoría de accesos y cambios (solo lectura, con búsqueda y paginación) | `AuditLogsController` → `/AuditLogs` |

**Nota sobre Citas:** los indicadores de "conteo de citas" y "ocupación" necesitan citas reales. Por eso se incluye un módulo mínimo
(agendar, listar y cambiar estado: atendida / cancelada / no asistió) que valida la disponibilidad del doctor y los cruces de horario.
No tiene videollamada, recordatorios ni facturación.

**Exclusiones respetadas:** no hay videollamadas, ni recordatorios/alertas/notificaciones, ni facturación, liquidaciones o comisiones.
Las tarifas son un campo de referencia que se configura y se muestra; ninguna pantalla suma dinero ni calcula ingresos.

---

## 2. Contenido del .zip

La carpeta `Psicologia.Web/` respeta la jerarquía del proyecto. Cópiala **encima** de tu carpeta `Psicologia.Web` (la del `.csproj`).

### 2.1 Archivos MODIFICADOS (reemplazan al original)

Se generaron a partir del `Clinica.zip` recibido. Si modificaste alguno de estos archivos después, **fusiona** en lugar de sobrescribir.

| Archivo | Cambio |
|---|---|
| `Data/Entities/User.cs` | `IsActive` (borrado lógico) y navegación `DoctorProfile` |
| `Data/Entities/Specialty.cs` | `Description`, `IsActive`, `Status` (aprobación) y datos de revisión |
| `Data/DataContext.cs` | Nuevos `DbSet`, relaciones, índices y valores por defecto |
| `Data/Seeders/RolesSeeder.cs` | Rol **Secretaria** |
| `Data/Seeders/PermissionsSeeder.cs` | Permisos de los nuevos módulos |
| `Data/Seeders/SeedDb.cs` | Llama a `ClinicalModulesSeeder` |
| `Core/Env.cs` | Constantes de roles y del permiso de aprobación |
| `Core/AutoMapperProfiles.cs` | Mapeos de servicios, agenda, citas y auditoría |
| `CustomConfiguration.cs` | Registro de los nuevos servicios (inyección de dependencias) |
| `Services/Abstractions/IAccountService.cs` + `Implementations/AccountService.cs` | Bloqueo de cuentas inactivas, auditoría de login/logout, `GetAccountUserAsync()` y `GetAccountDoctorProfileAsync()` públicos |
| `Services/Implementations/UsersService.cs` | Auditoría; no permite borrar físicamente usuarios con datos clínicos (sugiere desactivar) |
| `Services/Implementations/RolesService.cs` | Auditoría de creación/edición/eliminación de roles |
| `Services/Abstractions/ISpecialtiesService.cs` + `Implementations/SpecialtiesService.cs` | Aprobación, activar/desactivar, edición segura (ya no sobrescribe el estado) |
| `Controllers/SpecialtiesController.cs` | Acción `Toggle` |
| `Controllers/AccountController.cs` | Muestra el mensaje de "cuenta desactivada" |
| `Helpers/Abstractions/ISelectListHelper.cs` + `Implementations/SelectListHelper.cs` | Listas de especialidades, doctores, pacientes y servicios |
| `DTOs/Specialty/*.cs`, `DTOs/User/UserDTO.cs` | Nuevos campos |
| `Views/_ViewImports.cshtml` | `@using` de los nuevos DTOs, enums y extensiones |
| `Views/Shared/_Sidebar.cshtml` | Menú agrupado por módulo según permisos |
| `Views/Specialties/*.cshtml`, `Views/Users/Index.cshtml` | Descripción, estado y aprobación |

### 2.2 Archivos NUEVOS

- `Data/Entities/`: `DoctorProfile`, `ClinicalService`, `BusinessHour`, `DoctorAvailability`, `Appointment`, `ClinicalRecord`, `ClinicalNote`, `Diagnosis`, `AuditLog`
- `Data/Enums/`: `ApprovalStatus`, `AppointmentStatus`, `ClinicalNoteType`, `DiagnosisStatus`
- `Data/Seeders/ClinicalModulesSeeder.cs`
- `Core/`: `AuditActions.cs`, `OccupancyHelper.cs`, `Extensions/EnumExtensions.cs`, `Extensions/DateExtensions.cs`, `Extensions/StatusBadgeExtensions.cs`
- `DTOs/`: `Staff`, `Approval`, `Policy`, `Agenda`, `Appointment`, `ClinicalRecord`, `ClinicalService`, `DoctorPanel`, `Dashboard`, `Audit`, `Specialty/ToggleSpecialtyStatusDTO.cs`
- `Services/Abstractions` + `Services/Implementations`: `Audit`, `Staff`, `ClinicalServices`, `Dashboard`, `Approvals`, `Policies`, `Agenda`, `Appointments`, `ClinicalRecords`, `DoctorPanel`
- `Controllers/`: los 10 controladores de la tabla de la sección 1
- `Views/`: `Dashboard`, `Approvals`, `Policies`, `Agenda`, `Appointments`, `ClinicalRecords`, `DoctorPanel`, `Staff`, `ClinicalServices`, `AuditLogs`
- `wwwroot/assets/vendor/chart.js/chart.umd.js`: Chart.js 4.4.2 (licencia MIT), copiado de la misma plantilla NiceAdmin de PrivateBlog, para las gráficas del panel

---

## 3. Pasos de integración

1. **Copiar** la carpeta `Psicologia.Web/` del zip sobre la del proyecto (aceptar reemplazo de los archivos modificados).
2. **Compilar** la solución (`Ctrl+Shift+B`). No se agregan paquetes NuGet nuevos.
3. **Crear la migración.** Las migraciones existentes se conservan y la nueva va después de ellas:

   | Orden | Migración | Estado |
   |---|---|---|
   | 1 | `20261109000000_InitialCreate` | existente |
   | 2 | `20261110040825_UsersAndPermissions` | existente |
   | 3 | `ClinicalModules` | **nueva**: la generas tú |

   Consola del Administrador de paquetes (proyecto predeterminado: `Psicologia.Web`):
   ```powershell
   Add-Migration ClinicalModules
   Update-Database
   ```
   o con la CLI:
   ```bash
   dotnet ef migrations add ClinicalModules
   dotnet ef database update
   ```
   `Update-Database` es opcional: `SeedDb` ya ejecuta `Database.MigrateAsync()` al arrancar.
   Lo que **sí** es obligatorio es crear la migración antes de ejecutar: EF Core 9+ lanza una excepción en `MigrateAsync()` si el modelo tiene cambios sin migrar.

   Antes de aplicarla, revisa en el archivo generado que:
   - `AspNetUsers.IsActive` y `Specialties.IsActive` tengan `defaultValue: true` (así los usuarios y especialidades existentes quedan activos).
   - `Specialties.Status` tenga `defaultValue: 0` (0 = Aprobado, así las especialidades existentes quedan aprobadas).

   Las tablas nuevas son: `DoctorProfiles`, `BusinessHours`, `DoctorAvailabilities`, `Appointments`, `ClinicalRecords`, `ClinicalNotes`, `Diagnoses`, `ClinicalServices` y `AuditLogs`.

4. **Program.cs: sin cambios.** El proyecto registra todo en `CustomConfiguration.AddServices()`, que se llama desde `Program.cs` mediante `builder.AddCustomConfiguration()`. El archivo `CustomConfiguration.cs` del zip ya incluye:
   ```csharp
   // Administración del Sistema
   builder.Services.AddScoped<IAuditService, AuditService>();
   builder.Services.AddScoped<IStaffService, StaffService>();
   builder.Services.AddScoped<IClinicalServicesService, ClinicalServicesService>();

   // Panel de Dirección
   builder.Services.AddScoped<IDashboardService, DashboardService>();
   builder.Services.AddScoped<IApprovalsService, ApprovalsService>();
   builder.Services.AddScoped<IPoliciesService, PoliciesService>();

   // Atención Clínica
   builder.Services.AddScoped<IAgendaService, AgendaService>();
   builder.Services.AddScoped<IAppointmentsService, AppointmentsService>();
   builder.Services.AddScoped<IClinicalRecordsService, ClinicalRecordsService>();
   builder.Services.AddScoped<IDoctorPanelService, DoctorPanelService>();
   ```
5. **Ejecutar** la aplicación. Los seeders cargan los datos iniciales (sección 5).

---

## 4. Permisos

Se crean automáticamente con `PermissionsSeeder` y aparecen agrupados por módulo en **Roles → Editar**.
El rol **Administrador** tiene acceso total (super administrador), como ya funcionaba.

| Código | Módulo | Doctor | Secretaria |
|---|---|:-:|:-:|
| `dashboard.show` | Panel de Dirección | | |
| `approvals.show` | Aprobaciones | | |
| `approvals.manage` | Aprobaciones | | |
| `policies.show` / `policies.update` | Políticas | | |
| `agenda.show` | Agenda | ✓ | ✓ |
| `agenda.manage` | Agenda | ✓ | |
| `appointments.show` | Citas | ✓ | ✓ |
| `appointments.create` | Citas | | ✓ |
| `appointments.update` | Citas | ✓ | ✓ |
| `clinicalrecords.show` / `.create` / `.update` | Historia Clínica | ✓ | |
| `doctorpanel.show` | Panel del Doctor | ✓ | |
| `staff.show` / `.create` / `.update` / `.toggle` | Personal | | |
| `services.show` / `.create` / `.update` / `.delete` | Servicios | | |
| `audit.show` | Auditoría | | |

- Los permisos por defecto de Doctor y Secretaria **solo se asignan si el rol todavía no tiene ninguno de ellos**, para no deshacer la configuración que hagas desde la pantalla de Roles.
- `approvals.manage` define el flujo: quien lo tiene crea doctores y especialidades ya aprobados; quien no, los deja **pendientes** para la Dirección.
- Por compatibilidad, el rol Doctor conserva los permisos de Especialidades que ya tenía. Sin `approvals.manage`, un doctor solo puede *proponer* especialidades y editar o eliminar sus propias solicitudes pendientes.

---

## 5. Datos iniciales (`ClinicalModulesSeeder`)

Todos los pasos son idempotentes (solo insertan lo que falta):

- Horario de atención: lunes a viernes de 08:00 a 18:00, sábado de 08:00 a 12:00 y domingo cerrado.
- Servicios con tarifa de referencia: orientación inicial, psicoterapia individual, terapia de pareja y evaluación neuropsicológica.
- Perfil profesional **aprobado** para `doctor@psicologia.com`, con disponibilidad de lunes a viernes de 08:00 a 12:00 y de 14:00 a 18:00.
- Usuario **`secretaria@psicologia.com` / `1234`** con el rol Secretaria.

---

## 6. Reglas de negocio implementadas

- **Borrado lógico de personal:** al desactivar una cuenta se marca `IsActive = false` y se renueva el *security stamp* (las sesiones abiertas se invalidan). El login rechaza cuentas inactivas y una cookie vigente de una cuenta inactiva no tiene permisos. No se puede desactivar la cuenta propia.
- **Aprobaciones:** un doctor solo se puede aprobar si su especialidad está aprobada y activa. El rechazo exige observaciones. Los doctores pendientes pueden configurar su agenda, pero no recibir citas.
- **Agenda:** cada bloque debe quedar dentro del horario de atención de la clínica y no puede cruzarse con otro bloque del mismo día. Un doctor solo gestiona su propia agenda; administración y secretaría eligen el doctor.
- **Citas:** deben caber en la disponibilidad del doctor, sin cruces para el doctor ni para el paciente, y el servicio debe ser de la especialidad del doctor o general.
- **Historia clínica:** hay una por paciente (los pacientes son usuarios con rol *Paciente*, creados desde Usuarios). Un doctor ve solo las historias de pacientes que agendó o atendió, o que él abrió; el Administrador ve todas. Solo doctores aprobados escriben notas o diagnósticos, y cada uno edita únicamente lo suyo. Las notas no se eliminan.
- **Auditoría:** registra inicios de sesión (exitosos, fallidos y con cuenta desactivada), cierres de sesión y creación, edición, eliminación, activación, aprobación y rechazo en usuarios, roles, personal, especialidades, servicios, políticas, agenda, citas e historia clínica, con la IP. **Nunca guarda contenido clínico**, solo la acción realizada.
- **Ocupación** = minutos agendados (sin contar canceladas) ÷ minutos disponibles según la agenda semanal de doctores aprobados y activos en el periodo.

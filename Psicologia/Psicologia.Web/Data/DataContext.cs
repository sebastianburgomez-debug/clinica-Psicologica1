using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Data.Entities;

namespace Psicologia.Web.Data
{//puente entre framework y la base de datos, permite realizar operaciones de lectura y escritura en la base de datos
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // IdentityDbContext ya expone una propiedad "Roles" (AspNetRoles),
        // por eso los roles de la clínica se exponen como "PsicologiaRoles" (tabla "Roles")
        public DbSet<Role> PsicologiaRoles => Set<Role>();
        public DbSet<Specialty> Specialties => Set<Specialty>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        // Panel de Dirección
        public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
        public DbSet<BusinessHour> BusinessHours => Set<BusinessHour>();

        // Atención Clínica
        public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<ClinicalRecord> ClinicalRecords => Set<ClinicalRecord>();
        public DbSet<ClinicalNote> ClinicalNotes => Set<ClinicalNote>();
        public DbSet<Diagnosis> Diagnoses => Set<Diagnosis>();

        // Administración del Sistema
        public DbSet<ClinicalService> ClinicalServices => Set<ClinicalService>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ConfigureKeys(builder);
            ConfigureIndexes(builder);

            base.OnModelCreating(builder);
        }

        private void ConfigureKeys(ModelBuilder builder)
        {
            // Roles
            builder.Entity<Role>().ToTable("Roles");

            // User
            builder.Entity<User>().HasOne(u => u.Role)
                                  .WithMany(r => r.Users)
                                  .HasForeignKey(u => u.RoleId)
                                  .OnDelete(DeleteBehavior.Restrict);

            // Los usuarios existentes quedan activos al agregar la columna.
            // Sentinel = true: EF solo envía el valor en el INSERT cuando es false.
            builder.Entity<User>().Property(u => u.IsActive)
                                  .HasDefaultValue(true)
                                  .HasSentinel(true);

            // Role Permission
            builder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Entity<RolePermission>().HasOne(rp => rp.Permission)
                                            .WithMany(p => p.RolePermissions)
                                            .HasForeignKey(rp => rp.PermissionId);

            builder.Entity<RolePermission>().HasOne(rp => rp.Role)
                                            .WithMany(r => r.RolePermissions)
                                            .HasForeignKey(rp => rp.RoleId);

            // Specialty (las especialidades existentes quedan activas y aprobadas)
            builder.Entity<Specialty>().Property(s => s.IsActive)
                                       .HasDefaultValue(true)
                                       .HasSentinel(true);

            // Doctor Profile (1:1 con User)
            builder.Entity<DoctorProfile>().HasOne(d => d.User)
                                           .WithOne(u => u.DoctorProfile)
                                           .HasForeignKey<DoctorProfile>(d => d.UserId)
                                           .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DoctorProfile>().HasOne(d => d.Specialty)
                                           .WithMany(s => s.DoctorProfiles)
                                           .HasForeignKey(d => d.SpecialtyId)
                                           .OnDelete(DeleteBehavior.Restrict);

            // Clinical Service
            builder.Entity<ClinicalService>().HasOne(cs => cs.Specialty)
                                             .WithMany(s => s.ClinicalServices)
                                             .HasForeignKey(cs => cs.SpecialtyId)
                                             .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClinicalService>().Property(cs => cs.ReferenceRate)
                                             .HasPrecision(18, 2);

            // Doctor Availability
            builder.Entity<DoctorAvailability>().HasOne(a => a.DoctorProfile)
                                                .WithMany(d => d.Availabilities)
                                                .HasForeignKey(a => a.DoctorProfileId)
                                                .OnDelete(DeleteBehavior.Cascade);

            // Appointment
            builder.Entity<Appointment>().HasOne(a => a.DoctorProfile)
                                         .WithMany(d => d.Appointments)
                                         .HasForeignKey(a => a.DoctorProfileId)
                                         .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>().HasOne(a => a.Patient)
                                         .WithMany()
                                         .HasForeignKey(a => a.PatientId)
                                         .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>().HasOne(a => a.ClinicalService)
                                         .WithMany()
                                         .HasForeignKey(a => a.ClinicalServiceId)
                                         .OnDelete(DeleteBehavior.Restrict);

            // Clinical Record (1:1 con el paciente)
            builder.Entity<ClinicalRecord>().HasOne(r => r.Patient)
                                            .WithOne()
                                            .HasForeignKey<ClinicalRecord>(r => r.PatientId)
                                            .OnDelete(DeleteBehavior.Restrict);

            // Clinical Note
            builder.Entity<ClinicalNote>().HasOne(n => n.ClinicalRecord)
                                          .WithMany(r => r.Notes)
                                          .HasForeignKey(n => n.ClinicalRecordId)
                                          .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClinicalNote>().HasOne(n => n.DoctorProfile)
                                          .WithMany()
                                          .HasForeignKey(n => n.DoctorProfileId)
                                          .OnDelete(DeleteBehavior.Restrict);

            // Diagnosis
            builder.Entity<Diagnosis>().HasOne(d => d.ClinicalRecord)
                                       .WithMany(r => r.Diagnoses)
                                       .HasForeignKey(d => d.ClinicalRecordId)
                                       .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Diagnosis>().HasOne(d => d.DoctorProfile)
                                       .WithMany()
                                       .HasForeignKey(d => d.DoctorProfileId)
                                       .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureIndexes(ModelBuilder builder)
        {
            // Roles
            builder.Entity<Role>().HasIndex(r => r.Name)
                                  .IsUnique();

            // Specialties
            builder.Entity<Specialty>().HasIndex(s => s.Name)
                                       .IsUnique();

            // Permission
            builder.Entity<Permission>().HasIndex(p => p.Code)
                                        .IsUnique();

            // Doctor Profile
            builder.Entity<DoctorProfile>().HasIndex(d => d.ProfessionalLicense)
                                           .IsUnique();

            // Clinical Service
            builder.Entity<ClinicalService>().HasIndex(cs => cs.Name)
                                             .IsUnique();

            // Business Hour
            builder.Entity<BusinessHour>().HasIndex(b => b.DayOfWeek)
                                          .IsUnique();

            // Doctor Availability
            builder.Entity<DoctorAvailability>().HasIndex(a => new { a.DoctorProfileId, a.DayOfWeek });

            // Appointment
            builder.Entity<Appointment>().HasIndex(a => new { a.DoctorProfileId, a.StartsAt });
            builder.Entity<Appointment>().HasIndex(a => a.StartsAt);

            // Audit Log
            builder.Entity<AuditLog>().HasIndex(a => a.CreatedAt);
        }
    }
}

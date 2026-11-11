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

            // Role Permission
            builder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Entity<RolePermission>().HasOne(rp => rp.Permission)
                                            .WithMany(p => p.RolePermissions)
                                            .HasForeignKey(rp => rp.PermissionId);

            builder.Entity<RolePermission>().HasOne(rp => rp.Role)
                                            .WithMany(r => r.RolePermissions)
                                            .HasForeignKey(rp => rp.RoleId);
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
        }
    }
}


using Microsoft.EntityFrameworkCore;
using Psicologia.Data.Entities;

namespace Psicologia.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    // TABLAS PRINCIPALES
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Permiso> Permisos => Set<Permiso>();

    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<Modulo> Modulos => Set<Modulo>();

    public DbSet<Seccion> Secciones => Set<Seccion>();

    public DbSet<RolSeccion> RolSecciones => Set<RolSeccion>();
    // TABLA INTERMEDIA
    public DbSet<UsuarioEspecialidad> UsuarioEspecialidades
        => Set<UsuarioEspecialidad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ROLE
        modelBuilder.Entity<RolSeccion>(e =>
        {
            e.HasKey(rs => new { rs.RoleId, rs.SeccionId });

            e.HasOne(rs => rs.Role)
                .WithMany(r => r.RolSecciones)
                .HasForeignKey(rs => rs.RoleId);

            e.HasOne(rs => rs.Seccion)
                .WithMany(s => s.RolSecciones)
                .HasForeignKey(rs => rs.SeccionId);
        });
        modelBuilder.Entity<Seccion>(e =>
        {
            e.HasOne(s => s.Modulo)
                .WithMany(m => m.Secciones)
                .HasForeignKey(s => s.IdModulo);
        });
        modelBuilder.Entity<RolPermiso>(e =>
        {
            e.HasKey(rp => new { rp.RoleId, rp.PermisoId });

            e.HasOne(rp => rp.Role)
                .WithMany(r => r.RolPermisos)
                .HasForeignKey(rp => rp.RoleId);

            e.HasOne(rp => rp.Permiso)
                .WithMany(p => p.RolPermisos)
                .HasForeignKey(rp => rp.PermisoId);
        });
        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(r => r.Name).IsUnique();

            e.HasData(
                new Role { RoleId = 1, Name = "Administrador" },
                new Role { RoleId = 2, Name = "Doctor" },
                new Role { RoleId = 3, Name = "Paciente" },
                new Role { RoleId = 4, Name = "Secretaria" },
                new Role { RoleId = 5, Name = "Jefe" }
            );
        });

        // SPECIALTY
        modelBuilder.Entity<Specialty>(e =>
        {
            e.HasIndex(s => s.Name).IsUnique();

            e.HasData(
                new Specialty { SpecialtyId = 1, Name = "Psicología Clínica" },
                new Specialty { SpecialtyId = 2, Name = "Neuropsicología" }
            );
        });

        // USUARIO
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Correo).IsUnique();

            e.HasOne(u => u.Role)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RoleId);

            e.HasData(
                new Usuario
                {
                    UsuarioId = 1,
                    Name = "Katalina gonzalez",
                    Correo = "kata@gmail.com",
                    RoleId = 1
                }
            );
        });

        // USUARIO Y ESPECIALIDAD
        modelBuilder.Entity<UsuarioEspecialidad>(e =>
        {
            e.HasKey(ue => new
            {
                ue.UsuarioId,
                ue.SpecialtyId
            });

            e.HasOne(ue => ue.Usuario)
                .WithMany(u => u.UsuarioEspecialidades)
                .HasForeignKey(ue => ue.UsuarioId);

            e.HasOne(ue => ue.Specialty)
                .WithMany(s => s.UsuarioEspecialidades)
                .HasForeignKey(ue => ue.SpecialtyId);
        });
    }
}

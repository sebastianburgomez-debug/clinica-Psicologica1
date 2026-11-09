using Microsoft.EntityFrameworkCore;
using Psicologia.Data.Entities;

namespace Psicologia.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Specialty> Specialties => Set<Specialty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(r => r.Name).IsUnique();
            e.HasData(
                new Role { RoleId = 1, Name = "Administrador" },
                new Role { RoleId = 2, Name = "Doctor" },
                new Role { RoleId = 3, Name = "Paciente" });
        });

        modelBuilder.Entity<Specialty>(e =>
        {
            e.HasIndex(s => s.Name).IsUnique();
            e.HasData(
                new Specialty { SpecialtyId = 1, Name = "Psicología Clínica" },
                new Specialty { SpecialtyId = 2, Name = "Neuropsicología" });
        });
    }
}

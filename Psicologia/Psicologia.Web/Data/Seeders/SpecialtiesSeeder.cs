using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Data.Entities;

namespace Psicologia.Web.Data.Seeders
{
    public class SpecialtiesSeeder
    {
        private readonly DataContext _context;

        public SpecialtiesSeeder(DataContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            List<Specialty> specialties = new List<Specialty>
            {
                new Specialty { Id = Guid.NewGuid(), Name = "Psicología Clínica" },
                new Specialty { Id = Guid.NewGuid(), Name = "Neuropsicología" },
            };

            foreach (Specialty specialty in specialties)
            {
                bool exists = await _context.Specialties.AnyAsync(s => s.Name == specialty.Name);

                if (!exists)
                {
                    await _context.Specialties.AddAsync(specialty);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

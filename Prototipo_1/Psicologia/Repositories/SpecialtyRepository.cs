// Repositories/SpecialtyRepository.cs
using Psicologia.Data;
using Psicologia.Data.Entities;

namespace Psicologia.Repositories;

public class SpecialtyRepository : Repository<Specialty>, ISpecialtyRepository
{
    public SpecialtyRepository(DataContext context) : base(context) { }
}
using Psicologia.Data.Entities;

namespace Psicologia.Repositories;

public class SpecialtyRepository : ISpecialtyRepository
{
    private static readonly List<Specialty> _specialties = new()
    {
        new Specialty { SpecialtyId = 1, Name = "Psicología Clínica" },
        new Specialty { SpecialtyId = 2, Name = "Neuropsicología" }
    };

    public IEnumerable<Specialty> GetAll() => _specialties;
    public Specialty? GetById(int id) => _specialties.FirstOrDefault(s => s.SpecialtyId == id);
    public void Add(Specialty entity)
    {
        entity.SpecialtyId = _specialties.Count > 0 ? _specialties.Max(s => s.SpecialtyId) + 1 : 1;
        _specialties.Add(entity);
    }
    public void Delete(int id)
    {
        var item = GetById(id);
        if (item != null) _specialties.Remove(item);
    }
}
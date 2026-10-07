using Psicologia.Data.Entities;

namespace Psicologia.Repositories;

public class RoleRepository : IRoleRepository
{
    private static readonly List<Role> _roles = new()
    {
        new Role { RoleId = 1, Name = "Administrador" },
        new Role { RoleId = 2, Name = "Doctor" },
        new Role { RoleId = 3, Name = "Paciente" }
    };

    public IEnumerable<Role> GetAll() => _roles;
    public Role? GetById(int id) => _roles.FirstOrDefault(r => r.RoleId == id);
    public void Add(Role entity)
    {
        entity.RoleId = _roles.Count > 0 ? _roles.Max(r => r.RoleId) + 1 : 1;
        _roles.Add(entity);
    }
    public void Delete(int id)
    {
        var item = GetById(id);
        if (item != null) _roles.Remove(item);
    }
}

// Repositories/RoleRepository.cs
using Psicologia.Data;
using Psicologia.Data.Entities;

namespace Psicologia.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(DataContext context) : base(context) { }
}
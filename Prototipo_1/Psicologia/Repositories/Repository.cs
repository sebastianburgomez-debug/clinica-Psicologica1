// Repositories/Repository.cs
using Microsoft.EntityFrameworkCore;
using Psicologia.Data;

namespace Psicologia.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DataContext _context;
    protected readonly DbSet<T> _set;

    public Repository(DataContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync() =>
        await _set.AsNoTracking().ToListAsync();

    public virtual async Task<T?> GetByIdAsync(int id) =>
        await _set.FindAsync(id);

    public virtual async Task AddAsync(T entity)
    {
        await _set.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await _set.FindAsync(id);
        if (entity is null) return;

        _set.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
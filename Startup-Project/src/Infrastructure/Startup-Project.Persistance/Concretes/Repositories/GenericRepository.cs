using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities.Common;
using Startup_Project.Persistance.Data;
using System.Linq.Expressions;

namespace Startup_Project.Persistance.Concretes.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    public GenericRepository(ApplicationDbContext context) => _context = context;
    public DbSet<T> Table => _context.Set<T>();

    public async Task<bool> AddAsync(T entity)
    {
        var entityEntry = await Table.AddAsync(entity);
        return entityEntry.State == EntityState.Added;
    }

    public async Task<List<T>> GetAllAsync(bool tracking = true)
    {
        var query = Table.AsQueryable();
        if (!tracking)
            query = query.AsNoTracking();
        return await query.ToListAsync();
    }

    public async Task<T> GetByIdAsync(string id, bool tracking = true)
    {
        var query = Table.AsQueryable();
        if (!tracking)
            query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(data => data.Id == Guid.Parse(id));
    }
     
    public async Task<T> GetSingleAsync(Expression<Func<T, bool>> method, bool tracking = true)
    {
        var query = Table.AsQueryable();
        if (!tracking)
            query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(method);
    }

    public async Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> method, bool tracking = true)
    {
        var query = Table.AsQueryable();
        if (!tracking)
            query = query.AsNoTracking();
        return await query.Where(method).ToListAsync();
    }

    public bool Remove(T entity)
    {
        Table.Remove(entity);
        return true;
    }

    public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

    public bool Update(T entity)
    {
        Table.Update(entity);
        return true;
    }
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> method)
    {
        return await Table.AnyAsync(method);
    }
}

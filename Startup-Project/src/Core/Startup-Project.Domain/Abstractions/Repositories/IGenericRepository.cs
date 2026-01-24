

using Microsoft.EntityFrameworkCore;
using Startup_Project.Domain.Entities.Common;
using System.Linq.Expressions;

namespace Startup_Project.Domain.Abstractions.Repositories;

public interface IGenericRepository<T> where T : BaseEntity
{
    DbSet<T> Table { get; }
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync(bool tracking = true);
    Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> method, bool tracking = true);
    Task<T> GetByIdAsync(string id, bool tracking = true);
    Task<bool> AddAsync(T entity);
    bool Update(T entity);
    bool Remove(T entity);
    Task<int> SaveAsync();
    Task<T> GetSingleAsync(Expression<Func<T, bool>> method, bool tracking = true);
    Task<bool> AnyAsync(Expression<Func<T, bool>> method);
}



using Startup_Project.Domain.Entities.Common;
using System.Linq.Expressions;

namespace Startup_Project.Domain.Abstractions.Repositories;

public interface IGenericRepository<T> where T : BaseEntity
{
    List<T> GetAll(bool tracking = true);
    List<T> GetWhere(Expression<Func<T, bool>> method, bool tracking = true);
    Task<T> GetByIdAsync(string id, bool tracking = true);
    Task<bool> AddAsync(T entity);
    bool Update(T entity);
    bool Remove(T entity);
    Task<int> SaveAsync();
}

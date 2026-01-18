using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities.Common;
using Startup_Project.Persistance.Data;
using System.Linq.Expressions;

namespace Startup_Project.Persistance.Concretes.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    public GenericRepository(ApplicationDbContext context) => _context = context;

    public Task<bool> AddAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public List<T> GetAll(bool tracking = true)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetByIdAsync(string id, bool tracking = true)
    {
        throw new NotImplementedException();
    }

    public List<T> GetWhere(Expression<Func<T, bool>> method, bool tracking = true)
    {
        throw new NotImplementedException();
    }

    public bool Remove(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveAsync()
    {
        throw new NotImplementedException();
    }

    public bool Update(T entity)
    {
        throw new NotImplementedException();
    }
}

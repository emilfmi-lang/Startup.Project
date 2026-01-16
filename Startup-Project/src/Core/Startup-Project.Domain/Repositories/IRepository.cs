using Startup_Project.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Startup_Project.Domain.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task AddAsync(T entity);
    Task<T?> GetByIdAsync(Guid id);
}

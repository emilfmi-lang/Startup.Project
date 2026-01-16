using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Repositories;

namespace Startup_Project.Domain.Interfaces;

public interface IGymRepository: IRepository<Gym>
{
    Task<Gym> ExistsByNameAsync(string name);
}

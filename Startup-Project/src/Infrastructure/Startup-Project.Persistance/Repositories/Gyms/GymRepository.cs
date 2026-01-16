using Startup_Project.Application.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Startup_Project.Persistance.Repositories.Gyms;

public class GymRepository(IApplicationDbContext context):IGymRepository
{
    public async Task AddAsync(Gym entity) => await context.Gyms.AddAsync(entity);
    public async Task<bool> ExistsByNameAsync(string name) => await context.Gyms.AnyAsync(g => g.Name == name);

    public Task<Gym?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    Task<Gym> IGymRepository.ExistsByNameAsync(string name)
    {
        throw new NotImplementedException();
    }
}

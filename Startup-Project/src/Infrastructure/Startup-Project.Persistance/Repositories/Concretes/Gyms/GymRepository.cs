

using Startup_Project.Application.Interfaces;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Repositories.Concretes.Gyms;

public class GymRepository(IApplicationDbContext dbContext) : IGymRepository
{
    public async Task AddAsync(Gym gym)
    {
        await dbContext.Gyms.AddAsync(gym);
        await dbContext.SaveChangesAsync();
    }
}

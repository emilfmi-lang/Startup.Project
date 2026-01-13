
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Interfaces;

public interface IGymRepository
{
    Task AddAsync(Gym gym);
}

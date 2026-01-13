
using Microsoft.EntityFrameworkCore;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Gym> Gyms { get; set; }
    Task<int> SaveChangesAsync();
}

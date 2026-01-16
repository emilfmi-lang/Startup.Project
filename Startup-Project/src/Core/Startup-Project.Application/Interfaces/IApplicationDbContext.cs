
using Microsoft.EntityFrameworkCore;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Gym> Gyms { get; set; }
    DbSet<Subscription> Subscriptions { get; set; }
    DbSet<Package> Packages { get; set; }   
    Task<int> SaveChangesAsync();
}

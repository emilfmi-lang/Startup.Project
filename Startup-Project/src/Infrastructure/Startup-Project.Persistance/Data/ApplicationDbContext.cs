using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Interfaces;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Gym> Gyms { get; set; }
    public Task<int> SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }
}

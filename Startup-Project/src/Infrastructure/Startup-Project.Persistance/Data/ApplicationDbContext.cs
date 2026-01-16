using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Persistance.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser,IdentityRole<Guid>,Guid>, IApplicationDbContext
{
    public DbSet<Gym> Gyms { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Package> Packages { get; set; }
    public Task<int> SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }
}

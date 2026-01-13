using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Interfaces;

namespace Startup_Project.Persistance.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public Task<int> SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }
}

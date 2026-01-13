
namespace Startup_Project.Application.Interfaces;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync();
}

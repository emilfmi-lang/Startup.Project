namespace Startup_Project.Application.Abstractions.UnitOfWork;

public interface IUnitOfWork: IAsyncDisposable
{
    Task<int> SaveChangesAsync();
}

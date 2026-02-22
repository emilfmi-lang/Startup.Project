using Microsoft.EntityFrameworkCore.Storage;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Persistance.Data;

namespace Startup_Project.Persistance.Concretes;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException("Transaction basladilmayib.");

        await _context.SaveChangesAsync();
        await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        await _context.DisposeAsync();
    }
}

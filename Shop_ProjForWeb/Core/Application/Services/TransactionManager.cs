namespace Shop_ProjForWeb.Core.Application.Services;

using Microsoft.EntityFrameworkCore.Storage;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Infrastructure.Persistent.DbContext;

public class TransactionManager : ITransactionManager
{
    private readonly SupermarketDbContext _context;

    public TransactionManager(SupermarketDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await operation();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync(IDbContextTransaction transaction)
    {
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task RollbackAsync(IDbContextTransaction transaction)
    {
        await transaction.RollbackAsync();
    }
}
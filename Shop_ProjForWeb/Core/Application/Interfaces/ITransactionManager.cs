namespace Shop_ProjForWeb.Core.Application.Interfaces;

using Microsoft.EntityFrameworkCore.Storage;

public interface ITransactionManager
{
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    Task ExecuteInTransactionAsync(Func<Task> operation);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitAsync(IDbContextTransaction transaction);
    Task RollbackAsync(IDbContextTransaction transaction);
}
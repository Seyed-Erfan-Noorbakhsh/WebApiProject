namespace Shop_ProjForWeb.Core.Application.Interfaces;

using Shop_ProjForWeb.Core.Domain.Entities;

public interface IInventoryTransactionRepository
{
    Task<InventoryTransaction?> GetByIdAsync(Guid id);
    Task<List<InventoryTransaction>> GetAllAsync();
    Task<List<InventoryTransaction>> GetByInventoryIdAsync(Guid inventoryId);
    Task<List<InventoryTransaction>> GetByOrderIdAsync(Guid orderId);
    Task<List<InventoryTransaction>> GetByTransactionTypeAsync(string transactionType);
    Task<List<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddAsync(InventoryTransaction transaction);
    Task AddRangeAsync(IEnumerable<InventoryTransaction> transactions);
    Task UpdateAsync(InventoryTransaction transaction);
    Task DeleteAsync(Guid id);
}
namespace Shop_ProjForWeb.Core.Application.Interfaces;

using Shop_ProjForWeb.Core.Domain.Entities;

public interface IInventoryRepository
{
    Task<Inventory?> GetByProductIdAsync(Guid productId);
    Task UpdateAsync(Inventory inventory);
    Task AddAsync(Inventory inventory);
    Task DeleteAsync(Guid id);
    Task<List<Inventory>> GetAllAsync();
    Task<List<Inventory>> GetLowStockItemsAsync(int threshold = 10);
    Task<bool> CheckStockAvailabilityAsync(Guid productId, int quantity);
    
    /// <summary>
    /// Reserves stock with pessimistic locking to prevent race conditions.
    /// </summary>
    Task<Inventory?> ReserveStockAsync(Guid productId, int quantity);
    
    /// <summary>
    /// Decreases stock with pessimistic locking within a transaction.
    /// </summary>
    Task<bool> DecreaseStockWithLockAsync(Guid productId, int quantity);
}

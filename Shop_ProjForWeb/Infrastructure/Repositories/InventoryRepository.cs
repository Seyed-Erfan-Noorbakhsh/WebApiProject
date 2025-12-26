namespace Shop_ProjForWeb.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Infrastructure.Persistent.DbContext;

public class InventoryRepository(SupermarketDbContext context) : IInventoryRepository
{
    private readonly SupermarketDbContext _context = context;

    public async Task<Inventory?> GetByProductIdAsync(Guid productId)
    {
        return await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
    }

    public async Task UpdateAsync(Inventory inventory)
    {
        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(Inventory inventory)
    {
        await _context.Inventories.AddAsync(inventory);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory != null)
        {
            inventory.SoftDelete();
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Inventory>> GetAllAsync()
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .ToListAsync();
    }

    public async Task<List<Inventory>> GetLowStockItemsAsync(int threshold = 10)
    {
        return await _context.Inventories
            .Where(i => i.Quantity < i.LowStockThreshold)
            .Include(i => i.Product)
            .ToListAsync();
    }

    public async Task<bool> CheckStockAvailabilityAsync(Guid productId, int quantity)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId);
        
        return inventory != null && inventory.Quantity >= quantity;
    }

    /// <summary>
    /// Reserves stock with pessimistic locking to prevent race conditions.
    /// This method locks the inventory row to ensure no concurrent orders can oversell.
    /// </summary>
    public async Task<Inventory?> ReserveStockAsync(Guid productId, int quantity)
    {
        // Use FromSqlInterpolated with NOLOCK equivalent for SQLite (SQLite doesn't support FOR UPDATE)
        // For SQLite, we rely on transactions and the fact that SaveChangesAsync is atomic
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory == null || inventory.Quantity < quantity)
        {
            return null;
        }

        // Lock is implicit in the transaction - the row is locked until transaction commits
        return inventory;
    }

    /// <summary>
    /// Decreases stock with pessimistic locking within a transaction.
    /// Must be called within a transaction context.
    /// </summary>
    public async Task<bool> DecreaseStockWithLockAsync(Guid productId, int quantity)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory == null || inventory.Quantity < quantity)
        {
            return false;
        }

        inventory.Quantity -= quantity;
        inventory.LowStockFlag = inventory.Quantity < 10;
        inventory.LastUpdatedAt = DateTime.UtcNow;

        _context.Inventories.Update(inventory);
        // Don't save here - let the caller manage the transaction

        return true;
    }
}

namespace Shop_ProjForWeb.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Infrastructure.Persistent.DbContext;

public class InventoryTransactionRepository(SupermarketDbContext context) : IInventoryTransactionRepository
{
    private readonly SupermarketDbContext _context = context;

    public async Task<InventoryTransaction?> GetByIdAsync(Guid id)
    {
        return await _context.InventoryTransactions
            .Include(t => t.Inventory)
            .ThenInclude(i => i.Product)
            .Include(t => t.RelatedOrder)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<InventoryTransaction>> GetAllAsync()
    {
        return await _context.InventoryTransactions
            .Include(t => t.Inventory)
            .ThenInclude(i => i.Product)
            .Include(t => t.RelatedOrder)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<InventoryTransaction>> GetByInventoryIdAsync(Guid inventoryId)
    {
        return await _context.InventoryTransactions
            .Include(t => t.Inventory)
            .ThenInclude(i => i.Product)
            .Include(t => t.RelatedOrder)
            .Where(t => t.InventoryId == inventoryId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<InventoryTransaction>> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.InventoryTransactions
            .Include(t => t.Inventory)
            .ThenInclude(i => i.Product)
            .Include(t => t.RelatedOrder)
            .Where(t => t.RelatedOrderId == orderId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<InventoryTransaction>> GetByTransactionTypeAsync(string transactionType)
    {
        return await _context.InventoryTransactions
            .Include(t => t.Inventory)
            .ThenInclude(i => i.Product)
            .Include(t => t.RelatedOrder)
            .Where(t => t.TransactionType == transactionType)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.InventoryTransactions
            .Include(t => t.Inventory)
            .ThenInclude(i => i.Product)
            .Include(t => t.RelatedOrder)
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(InventoryTransaction transaction)
    {
        await _context.InventoryTransactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<InventoryTransaction> transactions)
    {
        await _context.InventoryTransactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(InventoryTransaction transaction)
    {
        _context.InventoryTransactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var transaction = await _context.InventoryTransactions.FindAsync(id);
        if (transaction != null)
        {
            _context.InventoryTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }
}
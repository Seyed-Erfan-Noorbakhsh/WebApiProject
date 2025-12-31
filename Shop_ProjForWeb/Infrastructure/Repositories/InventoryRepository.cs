namespace Shop_ProjForWeb.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Infrastructure.Persistent.DbContext;

public class InventoryRepository : IInventoryRepository
{
    private readonly SupermarketDbContext _context;

    public InventoryRepository(SupermarketDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByProductIdAsync(Guid productId)
    {
        return await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
    }

    public async Task UpdateAsync(Inventory inventory)
    {
        _context.Inventories.Update(inventory);
        await Task.CompletedTask;
    }
}

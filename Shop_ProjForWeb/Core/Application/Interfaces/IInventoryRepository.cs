namespace Shop_ProjForWeb.Core.Application.Interfaces;

using Shop_ProjForWeb.Core.Domain.Entities;

public interface IInventoryRepository
{
    Task<Inventory?> GetByProductIdAsync(Guid productId);
    Task UpdateAsync(Inventory inventory);
}

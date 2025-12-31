namespace Shop_ProjForWeb.Core.Application.Services;

using Shop_ProjForWeb.Core.Application.Interfaces;

public class InventoryService
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task DecreaseStockAsync(Guid productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);

        if (inventory == null)
        {
            throw new Exception($"Inventory not found for product {productId}");
        }

        if (quantity > inventory.Quantity)
        {
            throw new Exception($"Insufficient stock. Available: {inventory.Quantity}, Requested: {quantity}");
        }

        inventory.Quantity -= quantity;
        inventory.LowStockFlag = inventory.Quantity < 10;
        inventory.LastUpdatedAt = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(inventory);
    }
}

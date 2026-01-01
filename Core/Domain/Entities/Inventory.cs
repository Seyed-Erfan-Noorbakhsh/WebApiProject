namespace Shop_ProjForWeb.Core.Domain.Entities;

public class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 10; // Configurable per product
    public bool LowStockFlag { get; set; }
    public DateTime LastUpdatedAt { get; set; }

    // Navigation Properties
    public Product? Product { get; set; }

    public int AvailableQuantity => Quantity - ReservedQuantity;
}
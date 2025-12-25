namespace Shop_ProjForWeb.Core.Domain.Entities;


public class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public bool LowStockFlag { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

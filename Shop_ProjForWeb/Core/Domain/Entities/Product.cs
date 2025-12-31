namespace Shop_ProjForWeb.Core.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; }
    public decimal BasePrice { get; set; }
    public int DiscountPercent { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
}

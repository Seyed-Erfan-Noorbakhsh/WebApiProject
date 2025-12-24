namespace Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Core.Domain.Enums;


public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

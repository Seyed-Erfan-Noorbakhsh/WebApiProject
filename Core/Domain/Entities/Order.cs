namespace Shop_ProjForWeb.Core.Domain.Entities;
using Domain.Enums;
using Shop_ProjForWeb.Core.Domain.Interfaces;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public decimal TotalPrice { get; set; }
    public DateTime? PaidAt { get; set; }

    // Navigation Properties
    public User? User { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
namespace Shop_ProjForWeb.Core.Domain.Entities;
using Domain.Enums;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime? PaidAt { get; set; }
}

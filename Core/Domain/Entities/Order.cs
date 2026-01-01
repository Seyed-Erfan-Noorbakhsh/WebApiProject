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


    public override void ValidateEntity()
    {
        base.ValidateEntity();

        if (UserId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty");

        ValidateDecimalProperty(TotalPrice, nameof(TotalPrice), minValue: 0);

        if (Status == OrderStatus.Paid && PaidAt == null)
            throw new InvalidOperationException("Paid orders must have a PaidAt date");

        if (Status != OrderStatus.Paid && PaidAt != null)
            throw new InvalidOperationException("Only paid orders can have a PaidAt date");

        if (PaymentStatus == PaymentStatus.Success && Status != OrderStatus.Paid)
            throw new InvalidOperationException("Successful payment status requires paid order status");

        // Only validate order items if they have been loaded (not during initial creation)
        if (OrderItems != null && OrderItems.Any())
        {
            ValidateOrderItems();
        }
    }
    

    public void ChangeStatus(OrderStatus newStatus, IOrderStateMachine stateMachine)
    {
        if (stateMachine == null)
            throw new ArgumentNullException(nameof(stateMachine));
        
        stateMachine.ValidateBusinessRules(Status, newStatus, TotalPrice);
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        
        if (newStatus == OrderStatus.Paid)
        {
            PaidAt = DateTime.UtcNow;
            PaymentStatus = PaymentStatus.Success;
        }
        
        ValidateEntity();
    }
}
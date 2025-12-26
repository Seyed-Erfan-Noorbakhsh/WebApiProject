namespace Shop_ProjForWeb.Core.Domain.Entities;

public class InventoryTransaction : BaseEntity
{
    public Guid InventoryId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // "RESERVE", "COMMIT", "RELEASE", "ADJUST", "INCREASE", "DECREASE"
    public int Quantity { get; set; }
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? RelatedOrderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    // Navigation
    public Inventory Inventory { get; set; } = null!;
    public Order? RelatedOrder { get; set; }

    private static readonly string[] ValidTransactionTypes = { "RESERVE", "COMMIT", "RELEASE", "ADJUST", "INCREASE", "DECREASE" };

    public override void ValidateEntity()
    {
        base.ValidateEntity();
        
        if (InventoryId == Guid.Empty)
            throw new ArgumentException("InventoryId cannot be empty");
        
        ValidateStringProperty(TransactionType, nameof(TransactionType), minLength: 1, maxLength: 20);
        ValidateStringProperty(Reason, nameof(Reason), minLength: 1, maxLength: 500);
        ValidateStringProperty(UserId, nameof(UserId), minLength: 1, maxLength: 100);
        
        if (!ValidTransactionTypes.Contains(TransactionType))
            throw new ArgumentException($"Invalid transaction type: {TransactionType}. Valid types are: {string.Join(", ", ValidTransactionTypes)}");
        
        ValidateIntProperty(PreviousQuantity, nameof(PreviousQuantity), minValue: 0);
        ValidateIntProperty(NewQuantity, nameof(NewQuantity), minValue: 0);
        
        // Validate quantity change makes sense
        var expectedChange = NewQuantity - PreviousQuantity;
        if (TransactionType == "INCREASE" && expectedChange <= 0)
            throw new InvalidOperationException("INCREASE transactions must result in a positive quantity change");
        
        if (TransactionType == "DECREASE" && expectedChange >= 0)
            throw new InvalidOperationException("DECREASE transactions must result in a negative quantity change");
        
        if (TransactionType == "RESERVE" && expectedChange != 0)
            throw new InvalidOperationException("RESERVE transactions should not change total quantity");
        
        if (TransactionType == "COMMIT" && expectedChange >= 0)
            throw new InvalidOperationException("COMMIT transactions must result in a negative quantity change");
        
        if (TransactionType == "RELEASE" && expectedChange != 0)
            throw new InvalidOperationException("RELEASE transactions should not change total quantity");
    }

    public static InventoryTransaction CreateReservation(Guid inventoryId, int quantity, int previousQty, int newQty, Guid? orderId = null)
    {
        var transaction = new InventoryTransaction
        {
            InventoryId = inventoryId,
            TransactionType = "RESERVE",
            Quantity = quantity,
            PreviousQuantity = previousQty,
            NewQuantity = newQty,
            Reason = $"Reserved {quantity} items for order",
            RelatedOrderId = orderId,
            UserId = "System"
        };
        transaction.ValidateEntity();
        return transaction;
    }

    public static InventoryTransaction CreateCommit(Guid inventoryId, int quantity, int previousQty, int newQty, Guid? orderId = null)
    {
        var transaction = new InventoryTransaction
        {
            InventoryId = inventoryId,
            TransactionType = "COMMIT",
            Quantity = quantity,
            PreviousQuantity = previousQty,
            NewQuantity = newQty,
            Reason = $"Committed {quantity} items for order",
            RelatedOrderId = orderId,
            UserId = "System"
        };
        transaction.ValidateEntity();
        return transaction;
    }

    public static InventoryTransaction CreateRelease(Guid inventoryId, int quantity, int previousQty, int newQty, Guid? orderId = null)
    {
        var transaction = new InventoryTransaction
        {
            InventoryId = inventoryId,
            TransactionType = "RELEASE",
            Quantity = quantity,
            PreviousQuantity = previousQty,
            NewQuantity = newQty,
            Reason = $"Released {quantity} items from cancelled order",
            RelatedOrderId = orderId,
            UserId = "System"
        };
        transaction.ValidateEntity();
        return transaction;
    }

    public static InventoryTransaction CreateAdjustment(Guid inventoryId, int quantity, int previousQty, int newQty, string reason, string userId = "System")
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for adjustments");
        
        var transaction = new InventoryTransaction
        {
            InventoryId = inventoryId,
            TransactionType = "ADJUST",
            Quantity = quantity,
            PreviousQuantity = previousQty,
            NewQuantity = newQty,
            Reason = reason,
            UserId = userId
        };
        transaction.ValidateEntity();
        return transaction;
    }
}
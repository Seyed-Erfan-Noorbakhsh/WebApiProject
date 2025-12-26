namespace Shop_ProjForWeb.Core.Domain.Entities;

public class VipStatusHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public int PreviousTier { get; set; }
    public int NewTier { get; set; }
    public decimal TriggeringOrderTotal { get; set; }
    public decimal TotalSpendingAtUpgrade { get; set; }
    public string Reason { get; set; } = string.Empty;
    
    // Navigation
    public User User { get; set; } = null!;

    public override void ValidateEntity()
    {
        base.ValidateEntity();
        
        if (UserId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty");
        
        ValidateIntProperty(PreviousTier, nameof(PreviousTier), minValue: 0, maxValue: 10);
        ValidateIntProperty(NewTier, nameof(NewTier), minValue: 0, maxValue: 10);
        ValidateDecimalProperty(TriggeringOrderTotal, nameof(TriggeringOrderTotal), minValue: 0);
        ValidateDecimalProperty(TotalSpendingAtUpgrade, nameof(TotalSpendingAtUpgrade), minValue: 0);
        ValidateStringProperty(Reason, nameof(Reason), minLength: 1, maxLength: 500);
        
        if (PreviousTier == NewTier)
            throw new InvalidOperationException("VIP tier must change for a valid status history entry");
        
        if (NewTier < PreviousTier)
            throw new InvalidOperationException("VIP tier downgrades are not currently supported");
        
        if (PreviousTier == 0 && NewTier > 1)
            throw new InvalidOperationException("Users can only upgrade from regular (0) to VIP tier 1");
    }
}
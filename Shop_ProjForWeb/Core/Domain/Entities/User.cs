namespace Shop_ProjForWeb.Core.Domain.Entities;

public class User : BaseEntity
{
    public required string FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsVip { get; set; }
    public decimal TotalSpending { get; set; }
    public DateTime? VipUpgradedAt { get; set; }
    public int VipTier { get; set; } = 0; // 0 = Regular, 1 = VIP
    
    // Navigation Properties
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<VipStatusHistory> VipHistory { get; set; } = [];

    public override void ValidateEntity()
    {
        base.ValidateEntity();
        
        ValidateStringProperty(FullName, nameof(FullName), minLength: 2, maxLength: 100);
        
        if (!string.IsNullOrEmpty(Email))
        {
            ValidateStringProperty(Email, nameof(Email), maxLength: 255);
            if (!IsValidEmail(Email))
                throw new ArgumentException("Email format is invalid");
        }
        
        if (!string.IsNullOrEmpty(Phone))
        {
            ValidateStringProperty(Phone, nameof(Phone), maxLength: 20);
        }
        
        if (!string.IsNullOrEmpty(Address))
        {
            ValidateStringProperty(Address, nameof(Address), maxLength: 500);
        }
        
        ValidateDecimalProperty(TotalSpending, nameof(TotalSpending), minValue: 0);
        ValidateIntProperty(VipTier, nameof(VipTier), minValue: 0, maxValue: 10);
        
        if (IsVip && VipTier == 0)
            throw new InvalidOperationException("VIP users must have a VIP tier greater than 0");
        
        if (!IsVip && VipTier > 0)
            throw new InvalidOperationException("Non-VIP users cannot have a VIP tier");
        
        if (IsVip && VipUpgradedAt == null)
            throw new InvalidOperationException("VIP users must have a VipUpgradedAt date");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public void UpdateTotalSpending(decimal orderTotal)
    {
        if (orderTotal < 0)
            throw new ArgumentException("Order total cannot be negative");
        
        TotalSpending += orderTotal;
        UpdatedAt = DateTime.UtcNow;
        ValidateEntity();
    }

    public bool ShouldBeVip(decimal vipThreshold = 1000m)
    {
        return TotalSpending >= vipThreshold;
    }

    public void UpgradeToVip(decimal triggeringOrderTotal, string reason = "Spending threshold reached")
    {
        if (IsVip) return;

        if (triggeringOrderTotal < 0)
            throw new ArgumentException("Triggering order total cannot be negative");
        
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason for VIP upgrade is required");

        var previousTier = VipTier;
        IsVip = true;
        VipTier = 1;
        VipUpgradedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Create history record
        var history = new VipStatusHistory
        {
            UserId = Id,
            PreviousTier = previousTier,
            NewTier = VipTier,
            TriggeringOrderTotal = triggeringOrderTotal,
            TotalSpendingAtUpgrade = TotalSpending,
            Reason = reason
        };

        VipHistory.Add(history);
        ValidateEntity();
    }
}

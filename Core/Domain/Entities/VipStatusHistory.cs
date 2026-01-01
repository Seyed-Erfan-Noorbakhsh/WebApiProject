namespace Shop_ProjForWeb.Core.Domain.Entities;

public class VipStatusHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public int PreviousTier { get; set; }
    public int NewTier { get; set; }
    public decimal TriggeringOrderTotal { get; set; }
    public decimal TotalSpendingAtUpgrade { get; set; }
    public string Reason { get; set; } = string.Empty;

}
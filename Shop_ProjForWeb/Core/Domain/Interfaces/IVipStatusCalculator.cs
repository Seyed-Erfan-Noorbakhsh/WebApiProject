using Shop_ProjForWeb.Core.Domain.Entities;

namespace Shop_ProjForWeb.Core.Domain.Interfaces;

public interface IVipStatusCalculator
{
    bool ShouldBeVip(decimal totalPaidAmount, bool currentVipStatus);
    VipStatusChange CalculateStatusChange(User user, decimal newTotalAmount);
}

public class VipStatusChange
{
    public bool NewVipStatus { get; set; }
    public bool StatusChanged { get; set; }
    public string Reason { get; set; } = string.Empty;
}
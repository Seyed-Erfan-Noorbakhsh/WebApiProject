using Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Core.Domain.Interfaces;

namespace Shop_ProjForWeb.Core.Application.Services;

public class VipStatusCalculator : IVipStatusCalculator
{
    private const decimal VipUpgradeThreshold = 10000m;
    private const decimal VipDowngradeThreshold = 8000m; // Hysteresis gap

    public bool ShouldBeVip(decimal totalPaidAmount, bool currentVipStatus)
    {
        // Upgrade logic: Non-VIP becomes VIP at 10000
        if (!currentVipStatus && totalPaidAmount >= VipUpgradeThreshold)
        {
            return true;
        }

        // Downgrade logic: VIP becomes non-VIP below 8000
        if (currentVipStatus && totalPaidAmount < VipDowngradeThreshold)
        {
            return false;
        }

        // Hysteresis: Maintain current status between 8000-10000
        return currentVipStatus;
    }

    public VipStatusChange CalculateStatusChange(User user, decimal newTotalAmount)
    {
        var newVipStatus = ShouldBeVip(newTotalAmount, user.IsVip);
        var statusChanged = newVipStatus != user.IsVip;

        string reason = "";
        if (statusChanged)
        {
            if (newVipStatus)
            {
                reason = $"Upgraded to VIP: Total spending ${newTotalAmount:F2} reached threshold ${VipUpgradeThreshold:F2}";
            }
            else
            {
                reason = $"Downgraded from VIP: Total spending ${newTotalAmount:F2} fell below threshold ${VipDowngradeThreshold:F2}";
            }
        }

        return new VipStatusChange
        {
            NewVipStatus = newVipStatus,
            StatusChanged = statusChanged,
            Reason = reason
        };
    }
}
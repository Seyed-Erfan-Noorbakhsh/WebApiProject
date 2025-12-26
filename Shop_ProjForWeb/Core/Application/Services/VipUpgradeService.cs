namespace Shop_ProjForWeb.Core.Application.Services;

using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Exceptions;
using Shop_ProjForWeb.Core.Domain.Interfaces;

public class VipUpgradeService(
    IUserRepository userRepository, 
    IOrderRepository orderRepository, 
    IVipStatusCalculator vipStatusCalculator,
    IVipStatusHistoryRepository vipStatusHistoryRepository)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IVipStatusCalculator _vipStatusCalculator = vipStatusCalculator;
    private readonly IVipStatusHistoryRepository _vipStatusHistoryRepository = vipStatusHistoryRepository;
    private const decimal VIP_THRESHOLD = 1000m;

    public async Task CheckAndUpgradeAsync(Guid userId, decimal? orderTotal = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new UserNotFoundException($"User not found with id {userId}");
        }

        // Calculate total spending from all paid orders
        var totalPaidAmount = await _orderRepository.GetTotalPaidAmountForUserAsync(userId);
        
        // Update user's total spending
        user.TotalSpending = totalPaidAmount;

        var wasVip = user.IsVip;
        var shouldBeVip = totalPaidAmount >= VIP_THRESHOLD;

        Console.WriteLine($"VIP Check for user {userId}: TotalPaid={totalPaidAmount:C}, WasVip={wasVip}, ShouldBeVip={shouldBeVip}");

        if (!wasVip && shouldBeVip)
        {
            // Upgrade to VIP
            var triggeringOrderTotal = orderTotal ?? 0m;
            user.IsVip = true;
            user.VipTier = 1;
            user.VipUpgradedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            
            Console.WriteLine($"Upgrading user {userId} to VIP. IsVip={user.IsVip}, VipTier={user.VipTier}, VipUpgradedAt={user.VipUpgradedAt}");
            
            await _userRepository.UpdateAsync(user);
            
            // Verify the update
            var verifyUser = await _userRepository.GetByIdAsync(userId);
            Console.WriteLine($"After update verification: IsVip={verifyUser?.IsVip}, VipTier={verifyUser?.VipTier}");
            
            // Log the status change
            Console.WriteLine($"User {userId} upgraded to VIP. Total spending: {totalPaidAmount:C}");
        }
        else if (wasVip && !shouldBeVip)
        {
            // Downgrade from VIP (rare case, but possible after cancellations)
            user.IsVip = false;
            user.VipTier = 0;
            user.VipUpgradedAt = null;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepository.UpdateAsync(user);
            
            Console.WriteLine($"User {userId} downgraded from VIP. Total spending: {totalPaidAmount:C}");
        }
        else
        {
            // Just update total spending
            await _userRepository.UpdateAsync(user);
        }
    }

    public async Task RecalculateVipStatusAfterCancellation(Guid userId)
    {
        // Recalculate VIP status after order cancellation
        await CheckAndUpgradeAsync(userId);
    }

    public async Task<decimal> CalculateVipDiscountAsync(Guid userId, decimal baseAmount)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.IsVip != true)
        {
            return 0m;
        }

        // VIP gets 5% discount
        return baseAmount * 0.05m;
    }

    public async Task<bool> IsUserVipAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.IsVip == true;
    }

    public async Task<decimal> GetUserTotalSpendingAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.TotalSpending ?? 0m;
    }

    public async Task<decimal> GetRemainingAmountForVipAsync(Guid userId)
    {
        var totalSpending = await GetUserTotalSpendingAsync(userId);
        var remaining = VIP_THRESHOLD - totalSpending;
        return remaining > 0 ? remaining : 0m;
    }
}

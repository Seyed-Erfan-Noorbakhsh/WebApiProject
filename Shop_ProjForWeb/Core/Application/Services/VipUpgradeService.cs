namespace Shop_ProjForWeb.Core.Application.Services;

using Shop_ProjForWeb.Core.Application.Interfaces;

public class VipUpgradeService
{
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;

    public VipUpgradeService(IUserRepository userRepository, IOrderRepository orderRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
    }

    public async Task CheckAndUpgradeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new Exception($"User not found with id {userId}");
        }

        if (user.IsVip)
        {
            return;
        }

        var totalPaidAmount = await _orderRepository.GetTotalPaidAmountForUserAsync(userId);

        if (totalPaidAmount >= 10000)
        {
            user.IsVip = true;
            await _userRepository.UpdateAsync(user);
        }
    }
}

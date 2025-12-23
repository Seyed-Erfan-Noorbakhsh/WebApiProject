namespace Shop_ProjForWeb.Core.Application.Services;

using Shop_ProjForWeb.Core.Application.DTOs;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Core.Domain.Enums;

public class OrderService : IOrderService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly PricingService _pricingService;
    private readonly InventoryService _inventoryService;
    private readonly VipUpgradeService _vipUpgradeService;

    public OrderService(
        IUserRepository userRepository,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository,
        IOrderRepository orderRepository,
        PricingService pricingService,
        InventoryService inventoryService,
        VipUpgradeService vipUpgradeService)
    {
        _userRepository = userRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _orderRepository = orderRepository;
        _pricingService = pricingService;
        _inventoryService = inventoryService;
        _vipUpgradeService = vipUpgradeService;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(Guid userId, List<CreateOrderItemDto> items)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new Exception($"User not found with id {userId}");
        }

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Created,
            TotalPrice = 0
        };

        decimal totalPrice = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                throw new Exception($"Product not found with id {item.ProductId}");
            }

            var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId);
            if (inventory == null)
            {
                throw new Exception($"Inventory not found for product {item.ProductId}");
            }

            var unitPrice = _pricingService.CalculateFinalPrice(product.BasePrice, product.DiscountPercent, user.IsVip);

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                UnitPrice = unitPrice,
                Quantity = item.Quantity,
                DiscountApplied = product.DiscountPercent
            };

            orderItems.Add(orderItem);
            totalPrice += unitPrice * item.Quantity;

            await _inventoryService.DecreaseStockAsync(item.ProductId, item.Quantity);
        }

        order.TotalPrice = totalPrice;

        await _orderRepository.AddAsync(order);

        return new OrderResponseDto
        {
            OrderId = order.Id,
            TotalPrice = order.TotalPrice,
            Status = order.Status
        };
    }

    public async Task PayOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new Exception($"Order not found with id {orderId}");
        }

        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);

        await _vipUpgradeService.CheckAndUpgradeAsync(order.UserId);
    }
}

namespace Shop_ProjForWeb.Core.Application.Interfaces;

using Shop_ProjForWeb.Core.Application.DTOs;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(Guid userId, List<CreateOrderItemDto> items);
    Task PayOrderAsync(Guid orderId);
}

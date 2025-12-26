using MediatR;
using Shop_ProjForWeb.Core.Application.DTOs;

namespace Shop_ProjForWeb.Core.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<OrderResponseDto>
    {
        public int UserId { get; init; }
        public List<CreateOrderItemDto> Items { get; init; } = new();
    }
}

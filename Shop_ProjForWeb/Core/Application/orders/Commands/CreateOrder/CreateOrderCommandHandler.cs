using MediatR;
using Shop_ProjForWeb.Core.Application.DTOs;
using Shop_ProjForWeb.Core.Application.Interfaces;

namespace Shop_ProjForWeb.Core.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler
        : IRequestHandler<CreateOrderCommand, OrderResponseDto>
    {
        private readonly IOrderService _orderService;

        public CreateOrderCommandHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<OrderResponseDto> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            return await _orderService.CreateOrderAsync(
                request.UserId,
                request.Items);
        }
    }
}

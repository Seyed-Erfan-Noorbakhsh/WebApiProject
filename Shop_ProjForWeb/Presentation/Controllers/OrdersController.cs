using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shop_ProjForWeb.Core.Application.DTOs;
using Shop_ProjForWeb.Core.Application.Orders.Commands.CreateOrder;

namespace Shop_ProjForWeb.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var command = new CreateOrderCommand
            {
                UserId = request.UserId,
                Items = request.Items
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{orderId}/pay")]
        public async Task<IActionResult> PayOrder(int orderId)
        {
            //await _orderService.PayOrderAsync(orderId);
            return Ok();
        }
    }

    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
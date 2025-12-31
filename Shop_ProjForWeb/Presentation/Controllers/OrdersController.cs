namespace Shop_ProjForWeb.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using Shop_ProjForWeb.Core.Application.DTOs;
using Shop_ProjForWeb.Core.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrderAsync(request.UserId, request.Items);
        return Ok(result);
    }

    [HttpPost("{orderId}/pay")]
    public async Task<IActionResult> PayOrder(Guid orderId)
    {
        await _orderService.PayOrderAsync(orderId);
        return Ok();
    }
}

public class CreateOrderRequest
{
    public Guid UserId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; }
}

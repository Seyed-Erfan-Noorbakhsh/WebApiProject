namespace Shop_ProjForWeb.Core.Application.DTOs;

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

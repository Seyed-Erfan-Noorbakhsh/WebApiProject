using MediatR;
using Shop_ProjForWeb.Core.Application.DTOs;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Core.Domain.Enums;
using Shop_ProjForWeb.Core.Application.Services;


namespace Shop_ProjForWeb.Core.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler
        : IRequestHandler<CreateOrderCommand, OrderResponseDto>
    {
         private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly PricingService _pricingService;
        private readonly InventoryService _inventoryService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderCommandHandler(
             IUserRepository userRepository,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository,
            IOrderRepository orderRepository,
            PricingService pricingService,
            InventoryService inventoryService,
            IUnitOfWork unitOfWork)
        
        {
            _userRepository = userRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _orderRepository = orderRepository;
            _pricingService = pricingService;
            _inventoryService = inventoryService;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderResponseDto> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new Exception($"User not found with id {request.UserId}");

            var order = new Order
            {
                UserId = request.UserId,
                Status = OrderStatus.Created,
                TotalPrice = 0
            };

            decimal totalPrice = 0;

            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new Exception($"Product not found with id {item.ProductId}");

                var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId);
                if (inventory == null)
                    throw new Exception($"Inventory not found for product {item.ProductId}");

                var unitPrice = _pricingService.CalculateFinalPrice(
                    product.BasePrice,
                    product.DiscountPercent,
                    user.IsVip);

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    UnitPrice = unitPrice,
                    Quantity = item.Quantity,
                    DiscountApplied = product.DiscountPercent
                };

                order.OrderItems.Add(orderItem);

                totalPrice += unitPrice * item.Quantity;

                await _inventoryService.DecreaseStockAsync(item.ProductId, item.Quantity);
            }


            order.TotalPrice = totalPrice;

            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new OrderResponseDto
            {
                OrderId = order.Id,
                TotalPrice = order.TotalPrice,
                Status = order.Status
            };


            }

            
        }
    }


namespace Shop_ProjForWeb.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Core.Domain.Enums;
using Shop_ProjForWeb.Infrastructure.Data;


public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await Task.CompletedTask;
    }

    public async Task<decimal> GetTotalPaidAmountForUserAsync(Guid userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId && o.Status == OrderStatus.Paid)
            .SumAsync(o => o.TotalPrice);
    }
}

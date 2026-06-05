namespace PokeStore.Api.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;
using PokeStore.Api.Infrastructure.Data;

/// <summary>
/// Repository implementation for Order operations
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly PokestoreDbContext _context;

    public OrderRepository(PokestoreDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 20)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUserOrderCountAsync(int userId)
    {
        return await _context.Orders.CountAsync(o => o.UserId == userId);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }
}

namespace PokeStore.Api.Core.Interfaces;

using PokeStore.Api.Core.Entities;

/// <summary>
/// Repository interface for Order operations
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 20);
    Task<int> GetUserOrderCountAsync(int userId);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
}

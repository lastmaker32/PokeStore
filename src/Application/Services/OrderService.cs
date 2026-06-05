namespace PokeStore.Api.Application.Services;

using PokeStore.Api.Application.DTOs;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;

/// <summary>
/// Service for order management and retrieval
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    public async Task<OrderDTO?> GetOrderByIdAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId)
            return null;

        return MapToDTO(order);
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    public async Task<OrderDTO?> GetOrderByNumberAsync(int userId, string orderNumber)
    {
        var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
        if (order == null || order.UserId != userId)
            return null;

        return MapToDTO(order);
    }

    /// <summary>
    /// Get user's order history with pagination
    /// </summary>
    public async Task<OrderListResponseDTO> GetUserOrdersAsync(int userId, int pageNumber = 1, int pageSize = 20)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId, pageNumber, pageSize);
        var totalCount = await _orderRepository.GetUserOrderCountAsync(userId);
        
        return new OrderListResponseDTO
        {
            Orders = orders.Select(MapToDTO).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Update order status (admin only)
    /// </summary>
    public async Task<OrderDTO> UpdateOrderStatusAsync(int orderId, string newStatus)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        var validStatuses = new[] { "Pending", "PendingPayment", "Paid", "Shipped", "Delivered", "Cancelled" };
        if (!validStatuses.Contains(newStatus))
            throw new InvalidOperationException($"Invalid status: {newStatus}");

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);

        return MapToDTO(order);
    }

    /// <summary>
    /// Get invoice details
    /// </summary>
    public async Task<OrderDTO?> GetInvoiceAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId)
            return null;

        return MapToDTO(order);
    }

    private OrderDTO MapToDTO(Order order)
    {
        return new OrderDTO
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            SubtotalAmount = order.SubtotalAmount,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            BillingAddress = order.BillingAddress,
            Items = order.Items.Select(oi => new OrderItemDTO
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? string.Empty,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Total = oi.GetTotal()
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}

namespace PokeStore.Api.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeStore.Api.Application.DTOs;
using PokeStore.Api.Application.Services;
using System.Security.Claims;

/// <summary>
/// Controller for order management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrder(int orderId)
    {
        var userId = GetUserId();
        var order = await _orderService.GetOrderByIdAsync(userId, orderId);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        return Ok(order);
    }

    /// <summary>
    /// Get user's order history with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(OrderListResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _orderService.GetUserOrdersAsync(userId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrderByNumber(string orderNumber)
    {
        var userId = GetUserId();
        var order = await _orderService.GetOrderByNumberAsync(userId, orderNumber);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        return Ok(order);
    }

    /// <summary>
    /// Get invoice/receipt for order
    /// </summary>
    [HttpGet("{orderId}/invoice")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInvoice(int orderId)
    {
        var userId = GetUserId();
        var invoice = await _orderService.GetInvoiceAsync(userId, orderId);

        if (invoice == null)
            return NotFound(new { message = "Invoice not found" });

        return Ok(invoice);
    }

    /// <summary>
    /// Update order status (admin only)
    /// </summary>
    [HttpPut("{orderId}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequestDTO request)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(orderId, request.Status);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            throw new InvalidOperationException("Unable to extract user ID from token");

        return userId;
    }
}

public class UpdateOrderStatusRequestDTO
{
    public string Status { get; set; } = string.Empty;
}

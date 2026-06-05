namespace PokeStore.Api.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeStore.Api.Application.DTOs;
using PokeStore.Api.Application.Services;
using System.Security.Claims;

/// <summary>
/// Controller for shopping cart endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Get current user's cart
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CartDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();
        try
        {
            var cart = await _cartService.GetOrCreateCartAsync(userId);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(CartDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDTO request)
    {
        if (request.Quantity <= 0)
            return BadRequest(new { message = "Quantity must be greater than 0" });

        var userId = GetUserId();
        try
        {
            var cart = await _cartService.AddItemToCartAsync(userId, request.ProductId, request.Quantity);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    [HttpPut("items/{cartItemId}")]
    [ProducesResponseType(typeof(CartDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemRequestDTO request)
    {
        if (request.Quantity <= 0)
            return BadRequest(new { message = "Quantity must be greater than 0" });

        var userId = GetUserId();
        try
        {
            var cart = await _cartService.UpdateCartItemAsync(userId, cartItemId, request.Quantity);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    [HttpDelete("items/{cartItemId}")]
    [ProducesResponseType(typeof(CartDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        var userId = GetUserId();
        try
        {
            var cart = await _cartService.RemoveItemFromCartAsync(userId, cartItemId);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Clear entire cart
    /// </summary>
    [HttpDelete("clear")]
    [ProducesResponseType(typeof(CartDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        try
        {
            var cart = await _cartService.ClearCartAsync(userId);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get cart summary with totals
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(CartSummaryDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCartSummary()
    {
        var userId = GetUserId();
        try
        {
            var summary = await _cartService.GetCartSummaryAsync(userId);
            return Ok(summary);
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

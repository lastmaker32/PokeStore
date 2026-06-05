namespace PokeStore.Api.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeStore.Api.Application.DTOs;
using PokeStore.Api.Application.Services;
using System.Security.Claims;

/// <summary>
/// Controller for checkout operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly CheckoutService _checkoutService;

    public CheckoutController(CheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    /// <summary>
    /// Validate checkout (cart, inventory, addresses)
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(CheckoutValidationDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateCheckout([FromBody] CheckoutRequestDTO request)
    {
        var userId = GetUserId();
        try
        {
            var validation = await _checkoutService.ValidateCheckoutAsync(userId, request);
            return Ok(validation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Initiate payment - creates order with PendingPayment status
    /// </summary>
    [HttpPost("initiate-payment")]
    [ProducesResponseType(typeof(CreateOrderResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InitiatePayment([FromBody] CheckoutRequestDTO request)
    {
        var userId = GetUserId();
        try
        {
            var result = await _checkoutService.InitiatePaymentAsync(userId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Confirm payment - marks order as Paid and clears cart
    /// </summary>
    [HttpPost("payment-confirmation")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequestDTO request)
    {
        if (request.OrderId <= 0)
            return BadRequest(new { message = "Invalid OrderId" });

        var userId = GetUserId();
        try
        {
            var order = await _checkoutService.ConfirmPaymentAsync(userId, request.OrderId);
            return Ok(new { message = "Payment confirmed", order });
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

public class ConfirmPaymentRequestDTO
{
    public int OrderId { get; set; }
}

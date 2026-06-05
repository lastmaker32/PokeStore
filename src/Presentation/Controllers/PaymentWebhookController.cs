namespace PokeStore.Api.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using PokeStore.Api.Application.Services;
using PokeStore.Api.Infrastructure.Security;
using System.Collections.Generic;

/// <summary>
/// Controller for receiving and processing PayPal IPN (Instant Payment Notification) webhooks
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentWebhookController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly IWebhookSignatureValidator _signatureValidator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(
        PaymentService paymentService,
        IWebhookSignatureValidator signatureValidator,
        IConfiguration configuration,
        ILogger<PaymentWebhookController> logger)
    {
        _paymentService = paymentService;
        _signatureValidator = signatureValidator;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Receive PayPal IPN webhook notification
    /// </summary>
    /// <remarks>
    /// PayPal sends POST request with transaction data and signature.
    /// We verify the signature and process the payment event.
    /// Must return 200 OK immediately to acknowledge receipt.
    /// </remarks>
    [HttpPost("paypal/ipn")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReceivePayPalNotification()
    {
        try
        {
            // Read request body
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            _logger.LogInformation("Received PayPal IPN: {Body}", body);

            // Parse form data
            var postData = new Dictionary<string, string>();
            var pairs = body.Split('&');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    var key = System.Web.HttpUtility.UrlDecode(parts[0]);
                    var value = System.Web.HttpUtility.UrlDecode(parts[1]);
                    postData[key] = value;
                }
            }

            // Get signature from request headers
            var signature = Request.Headers["X-PAYPAL-TRANSMISSION-SIG"].ToString();
            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Missing PayPal signature header");
                return BadRequest("Missing signature");
            }

            // Verify signature
            var webhookSigningKey = _configuration["PayPal:WebhookSigningKey"] ?? string.Empty;
            if (!_signatureValidator.ValidateSignature(signature, postData, webhookSigningKey))
            {
                _logger.LogWarning("Invalid PayPal signature");
                return BadRequest("Invalid signature");
            }

            // Extract relevant fields
            var eventType = postData.ContainsKey("txn_type") ? postData["txn_type"] : "unknown";
            var transactionId = postData.ContainsKey("txn_id") ? postData["txn_id"] : string.Empty;
            var paymentStatus = postData.ContainsKey("payment_status") ? postData["payment_status"] : string.Empty;
            var customData = postData.ContainsKey("custom") ? postData["custom"] : string.Empty; // Order ID
            var amount = postData.ContainsKey("mc_gross") ? postData["mc_gross"] : "0";

            if (string.IsNullOrEmpty(transactionId) || string.IsNullOrEmpty(customData))
            {
                _logger.LogWarning("Missing required fields in PayPal IPN");
                return BadRequest("Missing required fields");
            }

            // Parse custom data (should contain orderId)
            if (!int.TryParse(customData, out var orderId))
            {
                _logger.LogWarning("Invalid order ID in custom field: {CustomData}", customData);
                return BadRequest("Invalid order ID");
            }

            // Parse amount
            if (!decimal.TryParse(amount, out var decimalAmount))
            {
                decimalAmount = 0;
            }

            // Process payment webhook
            var webhookId = Request.Headers["X-PAYPAL-TRANSMISSION-ID"].ToString();
            if (string.IsNullOrEmpty(webhookId))
            {
                webhookId = $"{transactionId}-{DateTime.UtcNow.Ticks}";
            }

            await _paymentService.ProcessPaymentWebhookAsync(
                webhookId,
                transactionId,
                orderId,
                eventType,
                paymentStatus,
                decimalAmount,
                body
            );

            _logger.LogInformation("PayPal IPN processed successfully. Transaction: {TransactionId}, Order: {OrderId}", 
                transactionId, orderId);

            // Return 200 OK to acknowledge receipt (PayPal won't retry)
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayPal IPN");
            // Still return 200 to prevent PayPal retries, but log the error
            return Ok();
        }
    }

    /// <summary>
    /// Health check endpoint for webhook delivery verification
    /// </summary>
    [HttpGet("paypal/health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}

namespace PokeStore.Api.Application.Services;

using PokeStore.Api.Application.DTOs;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;

/// <summary>
/// Service for payment processing and tracking
/// </summary>
public class PaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProcessedWebhookRepository _processedWebhookRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public PaymentService(
        IOrderRepository orderRepository,
        IProcessedWebhookRepository processedWebhookRepository,
        IInventoryRepository inventoryRepository)
    {
        _orderRepository = orderRepository;
        _processedWebhookRepository = processedWebhookRepository;
        _inventoryRepository = inventoryRepository;
    }

    /// <summary>
    /// Process webhook payment notification (idempotent)
    /// </summary>
    public async Task<bool> ProcessPaymentWebhookAsync(
        string webhookId,
        string transactionId,
        int orderId,
        string eventType,
        string status,
        decimal amount,
        string rawPayload)
    {
        // Check for duplicate webhook (idempotency)
        var existing = await _processedWebhookRepository.GetByWebhookIdAsync(webhookId);
        if (existing != null)
        {
            // Webhook already processed
            return existing.IsProcessed;
        }

        // Get order
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        // Create processed webhook record
        var processedWebhook = new ProcessedWebhook
        {
            WebhookId = webhookId,
            TransactionId = transactionId,
            OrderId = orderId,
            EventType = eventType,
            Status = status,
            Amount = amount,
            RawPayload = rawPayload,
            IsProcessed = false
        };

        // Handle different payment statuses
        if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            // Verify amount matches order total
            if (amount != order.TotalAmount)
                throw new InvalidOperationException($"Payment amount mismatch. Expected: {order.TotalAmount}, Received: {amount}");

            // Update order status
            order.Status = "Paid";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            // Create Payment record
            var payment = new Payment
            {
                OrderId = orderId,
                TransactionId = transactionId,
                Amount = amount,
                Status = "Completed",
                PaymentMethod = "PayPal",
                RawPayload = rawPayload,
                CreatedAt = DateTime.UtcNow
            };
            order.Payments.Add(payment);

            // Confirm inventory reservations (convert to permanent)
            foreach (var reservation in order.InventoryReservations)
            {
                await _inventoryRepository.ConfirmAsync(reservation.Id);
            }

            processedWebhook.IsProcessed = true;
            processedWebhook.ProcessedAt = DateTime.UtcNow;
        }
        else if (status.Equals("Failed", StringComparison.OrdinalIgnoreCase) ||
                 status.Equals("Denied", StringComparison.OrdinalIgnoreCase))
        {
            // Payment failed - release inventory
            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            // Release all inventory reservations
            foreach (var reservation in order.InventoryReservations.Where(r => r.Status == "Active"))
            {
                await _inventoryRepository.ReleaseAsync(reservation.Id);
            }

            // Create failed Payment record
            var payment = new Payment
            {
                OrderId = orderId,
                TransactionId = transactionId,
                Amount = amount,
                Status = "Failed",
                PaymentMethod = "PayPal",
                RawPayload = rawPayload,
                CreatedAt = DateTime.UtcNow
            };
            order.Payments.Add(payment);

            processedWebhook.IsProcessed = true;
            processedWebhook.ProcessedAt = DateTime.UtcNow;
        }
        else if (status.Equals("Refunded", StringComparison.OrdinalIgnoreCase))
        {
            // Payment refunded - update payment status
            var payment = order.Payments.FirstOrDefault(p => p.TransactionId == transactionId);
            if (payment != null)
            {
                payment.Status = "Refunded";
                payment.UpdatedAt = DateTime.UtcNow;
            }

            processedWebhook.IsProcessed = true;
            processedWebhook.ProcessedAt = DateTime.UtcNow;
        }

        // Save processed webhook
        await _processedWebhookRepository.CreateAsync(processedWebhook);
        return processedWebhook.IsProcessed;
    }

    /// <summary>
    /// Check if webhook was already processed (idempotency)
    /// </summary>
    public async Task<bool> IsWebhookProcessedAsync(string webhookId)
    {
        var webhook = await _processedWebhookRepository.GetByWebhookIdAsync(webhookId);
        return webhook?.IsProcessed ?? false;
    }

    /// <summary>
    /// Get payment details by transaction ID
    /// </summary>
    public async Task<ProcessedWebhook?> GetPaymentByTransactionIdAsync(string transactionId)
    {
        return await _processedWebhookRepository.GetByTransactionIdAsync(transactionId);
    }
}

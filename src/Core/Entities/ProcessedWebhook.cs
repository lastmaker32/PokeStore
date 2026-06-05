namespace PokeStore.Api.Core.Entities;

/// <summary>
/// Tracks processed webhooks for idempotency (prevents duplicate processing)
/// </summary>
public class ProcessedWebhook
{
    public int Id { get; set; }
    public string WebhookId { get; set; } = string.Empty; // Unique ID from PayPal
    public string TransactionId { get; set; } = string.Empty; // PayPal transaction ID
    public int OrderId { get; set; }
    public string EventType { get; set; } = string.Empty; // e.g., "web_accept", "subscr_payment"
    public string Status { get; set; } = string.Empty; // e.g., "Completed", "Failed"
    public decimal Amount { get; set; }
    public string RawPayload { get; set; } = string.Empty; // Full webhook JSON for audit
    public bool IsProcessed { get; set; }
    public DateTime ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Order Order { get; set; } = null!;
}

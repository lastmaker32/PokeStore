namespace PokeStore.Api.Core.Entities;

/// <summary>
/// Represents a payment transaction for an order (PayPal IPN)
/// </summary>
public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty; // PayPal txn_id
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
    public string PaymentMethod { get; set; } = "PayPal"; // PayPal, Stripe, etc.
    public string? PayerEmail { get; set; }
    public string? PayerName { get; set; }
    public string? RawPayload { get; set; } // Store raw IPN data for audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Order? Order { get; set; }
}

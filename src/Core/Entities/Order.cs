namespace PokeStore.Api.Core.Entities;

/// <summary>
/// Represents a customer order
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty; // PO-20260605-001, etc.
    public int UserId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, PendingPayment, Paid, Shipped, Delivered, Cancelled
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

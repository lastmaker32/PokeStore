namespace PokeStore.Api.Core.Entities;

/// <summary>
/// Represents a reservation of product inventory for a cart/order
/// </summary>
public class InventoryReservation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Quantity { get; set; }
    public int? CartId { get; set; }
    public int? OrderId { get; set; }
    public string Status { get; set; } = "Active"; // Active, Released, Confirmed
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; } // For temporary reservations (10-15 min)
    public DateTime? ReleasedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    // Navigation properties
    public virtual Product? Product { get; set; }
    public virtual User? User { get; set; }
    public virtual Cart? Cart { get; set; }
    public virtual Order? Order { get; set; }

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt;
}

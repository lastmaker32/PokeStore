namespace PokeStore.Api.Core.Entities;

/// <summary>
/// Represents a user's shopping cart
/// </summary>
public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    public virtual ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();

    public decimal GetSubtotal() => Items.Sum(item => item.GetTotal());
}

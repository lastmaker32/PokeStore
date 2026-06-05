namespace PokeStore.Api.Core.Entities;

/// <summary>
/// Represents a Pokémon product in the catalog
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; } = 0;
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Category? Category { get; set; }
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();

    /// <summary>
    /// Gets the available quantity (stock minus reserved items)
    /// </summary>
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
}

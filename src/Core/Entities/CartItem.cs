namespace PokeStore.Api.Core.Entities;

/// <summary>
/// Represents an item in a shopping cart
/// </summary>
public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Price at time of adding to cart
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Cart? Cart { get; set; }
    public virtual Product? Product { get; set; }

    public decimal GetTotal() => Quantity * UnitPrice;
}

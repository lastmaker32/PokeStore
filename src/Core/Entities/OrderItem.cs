namespace PokeStore.Api.Core.Entities;

/// <summary>
/// Represents an item in an order
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }

    public decimal GetTotal() => Quantity * UnitPrice;
}

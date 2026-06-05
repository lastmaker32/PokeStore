namespace PokeStore.Api.Application.DTOs;

public class CartItemDTO
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public int AvailableStock { get; set; }
}

public class CartDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<CartItemDTO> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AddToCartRequestDTO
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartItemRequestDTO
{
    public int CartItemId { get; set; }
    public int Quantity { get; set; }
}

public class CartSummaryDTO
{
    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

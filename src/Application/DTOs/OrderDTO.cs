namespace PokeStore.Api.Application.DTOs;

public class OrderItemDTO
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class OrderDTO
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public List<OrderItemDTO> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderListResponseDTO
{
    public List<OrderDTO> Orders { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

public class CheckoutRequestDTO
{
    public string ShippingAddress { get; set; } = string.Empty;
    public string? BillingAddress { get; set; }
}

public class CheckoutValidationDTO
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public decimal EstimatedTotal { get; set; }
}

public class CreateOrderResponseDTO
{
    public string OrderNumber { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Message { get; set; } = string.Empty;
}

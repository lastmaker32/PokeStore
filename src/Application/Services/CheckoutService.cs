namespace PokeStore.Api.Application.Services;

using PokeStore.Api.Application.DTOs;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;

/// <summary>
/// Service for checkout operations and validation
/// </summary>
public class CheckoutService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOrderRepository _orderRepository;

    public CheckoutService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository,
        IOrderRepository orderRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Validate checkout - cart, inventory, and customer data
    /// </summary>
    public async Task<CheckoutValidationDTO> ValidateCheckoutAsync(int userId, CheckoutRequestDTO request)
    {
        var errors = new List<string>();

        // Get cart
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null || !cart.Items.Any())
        {
            errors.Add("Cart is empty");
            return new CheckoutValidationDTO { IsValid = false, Errors = errors };
        }

        // Validate shipping address
        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            errors.Add("Shipping address is required");

        // Validate each cart item has available stock
        foreach (var item in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                errors.Add($"Product {item.ProductId} not found");
                continue;
            }

            var availableStock = await _inventoryRepository.GetAvailableStockAsync(item.ProductId);
            if (availableStock < item.Quantity)
            {
                errors.Add($"Insufficient stock for {product.Name}. Available: {availableStock}, Requested: {item.Quantity}");
            }

            // Check for price changes
            if (item.UnitPrice != product.Price)
            {
                errors.Add($"Price for {product.Name} has changed. Please review your cart");
            }
        }

        if (errors.Any())
            return new CheckoutValidationDTO { IsValid = false, Errors = errors };

        // Calculate estimated total
        var subtotal = cart.Items.Sum(i => i.GetTotal());
        var taxAmount = CalculateTax(subtotal);
        var shippingAmount = CalculateShipping(subtotal);
        var totalAmount = subtotal + taxAmount + shippingAmount;

        return new CheckoutValidationDTO
        {
            IsValid = true,
            Errors = new(),
            EstimatedTotal = totalAmount
        };
    }

    /// <summary>
    /// Initiate payment - prepare order for payment processing
    /// </summary>
    public async Task<CreateOrderResponseDTO> InitiatePaymentAsync(int userId, CheckoutRequestDTO request)
    {
        // Validate checkout first
        var validation = await ValidateCheckoutAsync(userId, request);
        if (!validation.IsValid)
            throw new InvalidOperationException($"Checkout validation failed: {string.Join(", ", validation.Errors)}");

        // Get cart
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null || !cart.Items.Any())
            throw new InvalidOperationException("Cart is empty");

        // Create order (PendingPayment status)
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            Status = "PendingPayment",
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress ?? request.ShippingAddress
        };

        // Add order items
        foreach (var cartItem in cart.Items)
        {
            var orderItem = new OrderItem
            {
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.UnitPrice
            };
            order.Items.Add(orderItem);
        }

        // Calculate totals
        order.SubtotalAmount = order.Items.Sum(oi => oi.GetTotal());
        order.TaxAmount = CalculateTax(order.SubtotalAmount);
        order.ShippingAmount = CalculateShipping(order.SubtotalAmount);
        order.TotalAmount = order.SubtotalAmount + order.TaxAmount + order.ShippingAmount;

        // Save order
        order = await _orderRepository.CreateAsync(order);

        // Create inventory reservations for order
        var expiresAt = DateTime.UtcNow.AddHours(1); // Order payment has 1 hour window
        foreach (var item in order.Items)
        {
            await _inventoryRepository.ReserveAsync(
                item.ProductId,
                userId,
                item.Quantity,
                expiresAt
            );
            // Link reservation to order (would need database update)
        }

        return new CreateOrderResponseDTO
        {
            OrderNumber = order.OrderNumber,
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            Message = $"Order {order.OrderNumber} created. Awaiting payment."
        };
    }

    /// <summary>
    /// Confirm payment and finalize order
    /// </summary>
    public async Task<OrderDTO> ConfirmPaymentAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        if (order.UserId != userId)
            throw new InvalidOperationException("Unauthorized access to order");

        if (order.Status != "PendingPayment")
            throw new InvalidOperationException($"Order cannot be paid - current status: {order.Status}");

        // Update order status
        order.Status = "Paid";
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order);

        // Clear user's cart (move items to order)
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart != null)
        {
            await _cartRepository.ClearCartAsync(cart.Id);
        }

        return MapToDTO(order);
    }

    private string GenerateOrderNumber()
    {
        // Format: PO-20260605-001
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"PO-{date}-{random}";
    }

    private decimal CalculateTax(decimal subtotal)
    {
        const decimal TAX_RATE = 0.10m; // 10% tax
        return subtotal * TAX_RATE;
    }

    private decimal CalculateShipping(decimal subtotal)
    {
        if (subtotal == 0) return 0;
        if (subtotal >= 100) return 0; // Free shipping over $100
        return 10m; // Flat $10 shipping
    }

    private OrderDTO MapToDTO(Order order)
    {
        return new OrderDTO
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            SubtotalAmount = order.SubtotalAmount,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            BillingAddress = order.BillingAddress,
            Items = order.Items.Select(oi => new OrderItemDTO
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? string.Empty,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Total = oi.GetTotal()
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}

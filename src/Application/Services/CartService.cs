namespace PokeStore.Api.Application.Services;

using PokeStore.Api.Application.DTOs;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;

/// <summary>
/// Service for shopping cart operations
/// </summary>
public class CartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private const int RESERVATION_DURATION_MINUTES = 15;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
    }

    /// <summary>
    /// Get or create a cart for the user
    /// </summary>
    public async Task<CartDTO> GetOrCreateCartAsync(int userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            cart = await _cartRepository.CreateAsync(cart);
        }

        return MapToDTO(cart);
    }

    /// <summary>
    /// Add item to cart with inventory reservation
    /// </summary>
    public async Task<CartDTO> AddItemToCartAsync(int userId, int productId, int quantity)
    {
        // Validate product exists and is active
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new InvalidOperationException($"Product {productId} not found");

        // Check available stock
        var availableStock = await _inventoryRepository.GetAvailableStockAsync(productId);
        if (availableStock < quantity)
            throw new InvalidOperationException($"Not enough stock. Available: {availableStock}, Requested: {quantity}");

        // Get or create cart
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            cart = await _cartRepository.CreateAsync(cart);
        }

        // Check if item already in cart
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            // Update quantity and reservation
            existingItem.Quantity += quantity;
            await _cartRepository.UpdateItemAsync(existingItem);
        }
        else
        {
            // Add new item
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price,
                AddedAt = DateTime.UtcNow
            };
            await _cartRepository.AddItemAsync(cartItem);
        }

        // Reserve inventory
        var expiresAt = DateTime.UtcNow.AddMinutes(RESERVATION_DURATION_MINUTES);
        await _inventoryRepository.ReserveAsync(productId, userId, quantity, expiresAt);

        // Update cart timestamp
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart);

        return await GetOrCreateCartAsync(userId);
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    public async Task<CartDTO> UpdateCartItemAsync(int userId, int cartItemId, int newQuantity)
    {
        if (newQuantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than 0");

        var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
        if (cartItem == null)
            throw new InvalidOperationException($"Cart item {cartItemId} not found");

        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null || cartItem.CartId != cart.Id)
            throw new InvalidOperationException("Cart item does not belong to user's cart");

        // Check available stock for new quantity
        var quantityDifference = newQuantity - cartItem.Quantity;
        if (quantityDifference > 0)
        {
            var availableStock = await _inventoryRepository.GetAvailableStockAsync(cartItem.ProductId);
            if (availableStock < quantityDifference)
                throw new InvalidOperationException($"Not enough stock for increase. Available: {availableStock}");
        }

        // Update quantity
        cartItem.Quantity = newQuantity;
        await _cartRepository.UpdateItemAsync(cartItem);

        // Update cart timestamp
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart);

        return await GetOrCreateCartAsync(userId);
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    public async Task<CartDTO> RemoveItemFromCartAsync(int userId, int cartItemId)
    {
        var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
        if (cartItem == null)
            throw new InvalidOperationException($"Cart item {cartItemId} not found");

        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null || cartItem.CartId != cart.Id)
            throw new InvalidOperationException("Cart item does not belong to user's cart");

        // Release inventory reservation
        var reservation = await _getReservationForCartItemAsync(cartItem);
        if (reservation != null)
        {
            await _inventoryRepository.ReleaseAsync(reservation.Id);
        }

        // Remove item
        await _cartRepository.DeleteItemAsync(cartItemId);

        // Update cart timestamp
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart);

        return await GetOrCreateCartAsync(userId);
    }

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    public async Task<CartDTO> ClearCartAsync(int userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
            throw new InvalidOperationException("Cart not found");

        // Release all reservations
        var reservations = await _getReservationsForCartAsync(cart.Id);
        foreach (var reservation in reservations)
        {
            await _inventoryRepository.ReleaseAsync(reservation.Id);
        }

        // Clear items
        await _cartRepository.ClearCartAsync(cart.Id);

        // Update cart timestamp
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart);

        return await GetOrCreateCartAsync(userId);
    }

    /// <summary>
    /// Get cart summary with calculated totals
    /// </summary>
    public async Task<CartSummaryDTO> GetCartSummaryAsync(int userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
            throw new InvalidOperationException("Cart not found");

        var subtotal = cart.Items.Sum(item => item.GetTotal());
        var taxAmount = CalculateTax(subtotal);
        var shippingAmount = CalculateShipping(subtotal);
        var totalAmount = subtotal + taxAmount + shippingAmount;

        return new CartSummaryDTO
        {
            ItemCount = cart.Items.Sum(i => i.Quantity),
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            ShippingAmount = shippingAmount,
            TotalAmount = totalAmount
        };
    }

    /// <summary>
    /// Calculate tax (simple flat rate for now)
    /// </summary>
    private decimal CalculateTax(decimal subtotal)
    {
        const decimal TAX_RATE = 0.10m; // 10% tax
        return subtotal * TAX_RATE;
    }

    /// <summary>
    /// Calculate shipping
    /// </summary>
    private decimal CalculateShipping(decimal subtotal)
    {
        if (subtotal == 0) return 0;
        if (subtotal >= 100) return 0; // Free shipping over $100
        return 10m; // Flat $10 shipping
    }

    private CartDTO MapToDTO(Cart cart)
    {
        return new CartDTO
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(item => new CartItemDTO
            {
                Id = item.Id,
                CartId = item.CartId,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                ProductImageUrl = item.Product?.ImageUrl,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Total = item.GetTotal(),
                AvailableStock = item.Product?.AvailableQuantity ?? 0
            }).ToList(),
            Subtotal = cart.GetSubtotal(),
            ItemCount = cart.Items.Sum(i => i.Quantity),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }

    private async Task<InventoryReservation?> _getReservationForCartItemAsync(CartItem cartItem)
    {
        // This would query the database for the reservation
        // For now, we'll return null - this is a placeholder
        return null;
    }

    private async Task<List<InventoryReservation>> _getReservationsForCartAsync(int cartId)
    {
        // This would query the database for all reservations for the cart
        // For now, we'll return empty list - this is a placeholder
        return new List<InventoryReservation>();
    }
}

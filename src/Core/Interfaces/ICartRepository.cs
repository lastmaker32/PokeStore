namespace PokeStore.Api.Core.Interfaces;

using PokeStore.Api.Core.Entities;

/// <summary>
/// Repository interface for Cart operations
/// </summary>
public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(int userId);
    Task<Cart> CreateAsync(Cart cart);
    Task<Cart> UpdateAsync(Cart cart);
    Task DeleteAsync(int cartId);
    Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
    Task<CartItem> AddItemAsync(CartItem item);
    Task<CartItem> UpdateItemAsync(CartItem item);
    Task DeleteItemAsync(int cartItemId);
    Task ClearCartAsync(int cartId);
}

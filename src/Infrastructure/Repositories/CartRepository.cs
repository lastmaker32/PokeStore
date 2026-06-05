namespace PokeStore.Api.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;
using PokeStore.Api.Infrastructure.Data;

/// <summary>
/// Repository implementation for Cart operations
/// </summary>
public class CartRepository : ICartRepository
{
    private readonly PokestoreDbContext _context;

    public CartRepository(PokestoreDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> CreateAsync(Cart cart)
    {
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<Cart> UpdateAsync(Cart cart)
    {
        cart.UpdatedAt = DateTime.UtcNow;
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task DeleteAsync(int cartId)
    {
        var cart = await _context.Carts.FindAsync(cartId);
        if (cart != null)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
    {
        return await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
    }

    public async Task<CartItem> AddItemAsync(CartItem item)
    {
        item.AddedAt = DateTime.UtcNow;
        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<CartItem> UpdateItemAsync(CartItem item)
    {
        _context.CartItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task DeleteItemAsync(int cartItemId)
    {
        var item = await _context.CartItems.FindAsync(cartItemId);
        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(int cartId)
    {
        var items = await _context.CartItems
            .Where(ci => ci.CartId == cartId)
            .ToListAsync();

        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}

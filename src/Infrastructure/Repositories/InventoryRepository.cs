namespace PokeStore.Api.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;
using PokeStore.Api.Infrastructure.Data;

/// <summary>
/// Repository implementation for Inventory operations
/// </summary>
public class InventoryRepository : IInventoryRepository
{
    private readonly PokestoreDbContext _context;

    public InventoryRepository(PokestoreDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetAvailableStockAsync(int productId)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId);

        return product?.AvailableQuantity ?? 0;
    }

    public async Task<InventoryReservation> ReserveAsync(int productId, int userId, int quantity, DateTime expiresAt)
    {
        var reservation = new InventoryReservation
        {
            ProductId = productId,
            UserId = userId,
            Quantity = quantity,
            Status = "Active",
            ExpiresAt = expiresAt,
            ReservedAt = DateTime.UtcNow
        };

        _context.InventoryReservations.Add(reservation);

        // Update product reserved quantity
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product != null)
        {
            product.ReservedQuantity += quantity;
        }

        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task ReleaseAsync(int reservationId)
    {
        var reservation = await _context.InventoryReservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null || reservation.Status != "Active")
            return;

        reservation.Status = "Released";
        reservation.ReleasedAt = DateTime.UtcNow;

        // Update product reserved quantity
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == reservation.ProductId);
        if (product != null)
        {
            product.ReservedQuantity -= reservation.Quantity;
        }

        await _context.SaveChangesAsync();
    }

    public async Task ConfirmAsync(int reservationId)
    {
        var reservation = await _context.InventoryReservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null || reservation.Status != "Active")
            return;

        reservation.Status = "Confirmed";
        reservation.ConfirmedAt = DateTime.UtcNow;

        // ReservedQuantity stays the same, it's now permanent (linked to order)

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<InventoryReservation>> GetExpiredReservationsAsync()
    {
        return await _context.InventoryReservations
            .Where(r => r.Status == "Active" && r.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task CleanupExpiredReservationsAsync()
    {
        var expiredReservations = await GetExpiredReservationsAsync();

        foreach (var reservation in expiredReservations)
        {
            await ReleaseAsync(reservation.Id);
        }
    }
}

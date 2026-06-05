namespace PokeStore.Api.Core.Interfaces;

using PokeStore.Api.Core.Entities;

/// <summary>
/// Repository interface for InventoryReservation operations
/// </summary>
public interface IInventoryRepository
{
    Task<int> GetAvailableStockAsync(int productId);
    Task<InventoryReservation> ReserveAsync(int productId, int userId, int quantity, DateTime expiresAt);
    Task ReleaseAsync(int reservationId);
    Task ConfirmAsync(int reservationId);
    Task<IEnumerable<InventoryReservation>> GetExpiredReservationsAsync();
    Task CleanupExpiredReservationsAsync();
}

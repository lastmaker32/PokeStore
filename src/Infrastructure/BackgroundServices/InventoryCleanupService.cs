namespace PokeStore.Api.Infrastructure.BackgroundServices;

using PokeStore.Api.Core.Interfaces;

/// <summary>
/// Background service for cleaning up expired inventory reservations
/// </summary>
public class InventoryCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InventoryCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

    public InventoryCleanupService(IServiceProvider serviceProvider, ILogger<InventoryCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InventoryCleanupService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during inventory cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("InventoryCleanupService stopped");
    }

    private async Task PerformCleanupAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var inventoryRepository = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
            
            _logger.LogInformation("Starting inventory cleanup at {time}", DateTime.UtcNow);
            
            var expiredReservations = await inventoryRepository.GetExpiredReservationsAsync();
            
            if (expiredReservations.Any())
            {
                _logger.LogInformation("Found {count} expired reservations to clean up", expiredReservations.Count());
                
                foreach (var reservation in expiredReservations)
                {
                    await inventoryRepository.ReleaseAsync(reservation.Id);
                }
                
                _logger.LogInformation("Cleaned up {count} expired reservations", expiredReservations.Count());
            }
        }
    }
}

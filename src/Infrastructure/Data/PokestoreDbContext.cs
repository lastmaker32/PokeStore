namespace PokeStore.Api.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Infrastructure.Data.Configurations;

/// <summary>
/// POKESTORE database context using Entity Framework Core
/// </summary>
public class PokestoreDbContext : DbContext
{
    public PokestoreDbContext(DbContextOptions<PokestoreDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<InventoryReservation> InventoryReservations { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ProcessedWebhook> ProcessedWebhooks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new CartConfiguration());
        modelBuilder.ApplyConfiguration(new CartItemConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryReservationConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedWebhookConfiguration());

        // EXACT FIX FOR THE CASCADE PATH CRASH (Placed here to override any conflicting configurations)
        modelBuilder.Entity<InventoryReservation>()
            .HasOne(i => i.Order)
            .WithMany() 
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<InventoryReservation>()
            .HasOne(i => i.Cart)
            .WithMany() 
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

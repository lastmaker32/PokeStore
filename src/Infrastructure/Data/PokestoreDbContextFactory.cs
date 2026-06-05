namespace PokeStore.Api.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Design-time factory for creating DbContext instances during migrations
/// </summary>
public class PokestoreDbContextFactory : IDesignTimeDbContextFactory<PokestoreDbContext>
{
    public PokestoreDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PokestoreDbContext>();
        
        // Use LocalDB for development
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=PokestoreDb;Trusted_Connection=true;");
        
        return new PokestoreDbContext(optionsBuilder.Options);
    }
}

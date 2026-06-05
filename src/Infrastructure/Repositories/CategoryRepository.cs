namespace PokeStore.Api.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;
using PokeStore.Api.Infrastructure.Data;

/// <summary>
/// Repository implementation for Category operations
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly PokestoreDbContext _context;

    public CategoryRepository(PokestoreDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}

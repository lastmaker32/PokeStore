namespace PokeStore.Api.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;
using PokeStore.Api.Infrastructure.Data;

/// <summary>
/// Repository implementation for Product operations
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly PokestoreDbContext _context;

    public ProductRepository(PokestoreDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId, int pageNumber = 1, int pageSize = 20)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string query, int pageNumber = 1, int pageSize = 20)
    {
        var searchTerm = query.ToLower();
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => (p.Name.ToLower().Contains(searchTerm) || 
                        p.Description.ToLower().Contains(searchTerm)) && 
                       p.IsActive)
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            product.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}

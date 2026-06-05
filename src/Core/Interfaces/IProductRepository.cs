namespace PokeStore.Api.Core.Interfaces;

using PokeStore.Api.Core.Entities;

/// <summary>
/// Repository interface for Product operations
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync(int pageNumber = 1, int pageSize = 20);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId, int pageNumber = 1, int pageSize = 20);
    Task<IEnumerable<Product>> SearchAsync(string query, int pageNumber = 1, int pageSize = 20);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(int id);
}

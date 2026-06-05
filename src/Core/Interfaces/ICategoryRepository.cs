namespace PokeStore.Api.Core.Interfaces;

using PokeStore.Api.Core.Entities;

/// <summary>
/// Repository interface for Category operations
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllAsync();
}

namespace PokeStore.Api.Core.Interfaces;

using PokeStore.Api.Core.Entities;

/// <summary>
/// Repository interface for User operations
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
}

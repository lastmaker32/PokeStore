namespace PokeStore.Api.Core.Interfaces;

/// <summary>
/// Service interface for password hashing operations
/// </summary>
public interface IPasswordHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

namespace PokeStore.Api.Infrastructure.Security;

using BC = BCrypt.Net.BCrypt;
using PokeStore.Api.Core.Interfaces;

/// <summary>
/// Password hashing service using BCrypt
/// </summary>
public class PasswordHashService : IPasswordHashService
{
    public string HashPassword(string password)
    {
        return BC.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BC.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}

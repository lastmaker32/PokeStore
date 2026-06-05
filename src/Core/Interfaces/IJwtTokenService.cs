namespace PokeStore.Api.Core.Interfaces;

/// <summary>
/// Service interface for JWT token operations
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(int userId, string email, string role);
    string GenerateRefreshToken();
    (int UserId, string Email, string Role) ValidateToken(string token);
}

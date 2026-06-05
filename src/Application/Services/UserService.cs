namespace PokeStore.Api.Application.Services;

using PokeStore.Api.Application.DTOs;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;

/// <summary>
/// Service for user authentication and registration
/// </summary>
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _tokenService;
    private readonly IPasswordHashService _passwordService;

    public UserService(
        IUserRepository userRepository,
        IJwtTokenService tokenService,
        IPasswordHashService passwordService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new InvalidOperationException("Email and password are required");

        if (request.Password != request.ConfirmPassword)
            throw new InvalidOperationException("Passwords do not match");

        if (request.Password.Length < 6)
            throw new InvalidOperationException("Password must be at least 6 characters");

        // Check if user exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        // Create user
        var user = new User
        {
            Email = request.Email.ToLower(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = "User",
            IsActive = true
        };

        user = await _userRepository.CreateAsync(user);

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new InvalidOperationException("Email and password are required");

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Invalid email or password");

        if (!user.IsActive)
            throw new InvalidOperationException("User account is inactive");

        return GenerateAuthResponse(user);
    }

    public async Task<UserDTO?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user == null ? null : MapToDTO(user);
    }

    private AuthResponseDTO GenerateAuthResponse(User user)
    {
        return new AuthResponseDTO
        {
            User = MapToDTO(user),
            AccessToken = _tokenService.GenerateToken(user.Id, user.Email, user.Role),
            RefreshToken = _tokenService.GenerateRefreshToken()
        };
    }

    private UserDTO MapToDTO(User user)
    {
        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }
}

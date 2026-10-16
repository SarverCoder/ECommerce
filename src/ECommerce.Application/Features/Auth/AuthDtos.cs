namespace ECommerce.Application.Features.Auth;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber,
    string? Address);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    Guid UserId,
    string Email,
    string Role,
    Guid? CustomerId,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);

public record CurrentUserDto(Guid UserId, string Email, string Role, Guid? CustomerId);

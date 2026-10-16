using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Authentication;

public record AccessToken(string Token, DateTime ExpiresAt);

public record RefreshToken(string Token, string Hash, DateTime ExpiresAt);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
    RefreshToken CreateRefreshToken();
    string HashRefreshToken(string token);
}

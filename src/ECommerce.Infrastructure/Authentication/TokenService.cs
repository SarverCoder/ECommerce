using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Authentication;

public class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    public const string RoleClaim = "role";
    public const string CustomerIdClaim = "customer_id";

    private readonly JwtOptions _options = options.Value;
    private readonly JsonWebTokenHandler _handler = new();

    public AccessToken CreateAccessToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
            [JwtRegisteredClaimNames.Email] = user.Email,
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            [RoleClaim] = user.Role.ToString()
        };
        if (user.CustomerId is { } customerId)
            claims[CustomerIdClaim] = customerId.ToString();

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expiresAt,
            Claims = claims,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
                SecurityAlgorithms.HmacSha256)
        });

        return new AccessToken(token, expiresAt);
    }

    public RefreshToken CreateRefreshToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return new RefreshToken(token, HashRefreshToken(token), DateTime.UtcNow.AddDays(_options.RefreshTokenDays));
    }

    public string HashRefreshToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

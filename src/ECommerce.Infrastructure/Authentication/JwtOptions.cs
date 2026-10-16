namespace ECommerce.Infrastructure.Authentication;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public string Secret { get; init; } = null!;
    public int AccessTokenMinutes { get; init; } = 60;
    public int RefreshTokenDays { get; init; } = 7;
}

using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ECommerce.Api.Authentication;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private System.Security.Claims.ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid? UserId => ParseGuid(JwtRegisteredClaimNames.Sub);

    public Guid? CustomerId => ParseGuid(TokenService.CustomerIdClaim);

    public bool IsAdmin => Principal?.IsInRole(nameof(UserRole.Admin)) == true;

    private Guid? ParseGuid(string claimType)
        => Guid.TryParse(Principal?.FindFirst(claimType)?.Value, out var id) ? id : null;
}

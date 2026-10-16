using ECommerce.Domain.Enums;

namespace ECommerce.Api.Authentication;

public static class Roles
{
    public const string Admin = nameof(UserRole.Admin);
    public const string Customer = nameof(UserRole.Customer);
}

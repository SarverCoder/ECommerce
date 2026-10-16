using FluentValidation;

namespace ECommerce.Application.Common.Validation;

/// <summary>Reusable rules shared by several validators.</summary>
public static class ValidationRules
{
    public static IRuleBuilderOptions<T, string> PersonName<T>(this IRuleBuilder<T, string> rule)
        => rule.NotEmpty().MaximumLength(100)
            .Matches(@"^[\p{L}][\p{L}\s'\-]*$").WithMessage("'{PropertyName}' may contain only letters, spaces, apostrophes and hyphens.");

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
        => rule.NotEmpty().MaximumLength(256).EmailAddress();

    public static IRuleBuilderOptions<T, string?> PhoneNumber<T>(this IRuleBuilder<T, string?> rule)
        => rule.Matches(@"^\+?[0-9]{9,15}$").WithMessage("'{PropertyName}' must be 9-15 digits, optionally starting with '+'.");

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule)
        => rule.NotEmpty()
            .MinimumLength(8).MaximumLength(100)
            .Matches("[A-Z]").WithMessage("'{PropertyName}' must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("'{PropertyName}' must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("'{PropertyName}' must contain a digit.");

    public static IRuleBuilderOptions<T, decimal> Money<T>(this IRuleBuilder<T, decimal> rule)
        => rule.GreaterThan(0).LessThanOrEqualTo(1_000_000_000).PrecisionScale(18, 2, ignoreTrailingZeros: true);
}

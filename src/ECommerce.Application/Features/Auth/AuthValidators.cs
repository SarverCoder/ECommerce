using ECommerce.Application.Common.Validation;
using FluentValidation;

namespace ECommerce.Application.Features.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName).PersonName();
        RuleFor(x => x.LastName).PersonName();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).StrongPassword();
        RuleFor(x => x.PhoneNumber).PhoneNumber();
        RuleFor(x => x.Address).MaximumLength(500);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(200);
    }
}

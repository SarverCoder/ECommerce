using ECommerce.Application.Common.Validation;
using FluentValidation;

namespace ECommerce.Application.Features.Customers;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.FirstName).PersonName();
        RuleFor(x => x.LastName).PersonName();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.PhoneNumber).PhoneNumber();
        RuleFor(x => x.Address).MaximumLength(500);
    }
}

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(x => x.FirstName).PersonName();
        RuleFor(x => x.LastName).PersonName();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.PhoneNumber).PhoneNumber();
        RuleFor(x => x.Address).MaximumLength(500);
    }
}

using FluentValidation;

namespace ECommerce.Application.Features.Orders;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    private const int MaxItems = 50;

    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEqual(Guid.Empty).When(x => x.CustomerId.HasValue);

        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(500);

        RuleFor(x => x.Items)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .Must(items => items.Count <= MaxItems).WithMessage($"Order cannot contain more than {MaxItems} items.")
            .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
                .WithMessage("Each product may appear only once; increase its quantity instead.");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemRequestValidator());
    }
}

public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).InclusiveBetween(1, 1000);
    }
}

using ECommerce.Application.Common.Validation;
using FluentValidation;

namespace ECommerce.Application.Features.Products;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Price).Money();
        RuleFor(x => x.StockQuantity).InclusiveBetween(0, 1_000_000);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Price).Money();
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.Quantity).InclusiveBetween(1, 1_000_000);
    }
}

public class ProductQueryValidator : AbstractValidator<ProductQuery>
{
    public ProductQueryValidator()
    {
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

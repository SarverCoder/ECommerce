using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId,
    string? CategoryName,
    string? ImageUrl,
    DateTime CreatedAt)
{
    public static ProductDto From(Product p, string? imageUrl) => new(
        p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.IsActive,
        p.CategoryId, p.Category?.Name, imageUrl, p.CreatedAt);
}

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    Guid CategoryId);

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    Guid CategoryId);

public record AdjustStockRequest(int Quantity);

public record ProductQuery(string? Search, Guid? CategoryId, int Page = 1, int PageSize = 20);

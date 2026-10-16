using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Categories;

public record CategoryDto(Guid Id, string Name, string? Description, DateTime CreatedAt)
{
    public static CategoryDto From(Category c) => new(c.Id, c.Name, c.Description, c.CreatedAt);
}

public record CreateCategoryRequest(string Name, string? Description);

public record UpdateCategoryRequest(string Name, string? Description);

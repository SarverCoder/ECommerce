using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default);
    Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public class CategoryService(ICategoryRepository categories, IUnitOfWork unitOfWork) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await categories.GetAllAsync(ct);
        return items.Select(CategoryDto.From).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        => CategoryDto.From(await GetOrThrowAsync(id, ct));

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        if (await categories.NameExistsAsync(request.Name, ct: ct))
            throw new ConflictException($"Category '{request.Name}' already exists.");

        var category = new Category(request.Name, request.Description);
        await categories.AddAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return CategoryDto.From(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await GetOrThrowAsync(id, ct);
        if (await categories.NameExistsAsync(request.Name, id, ct))
            throw new ConflictException($"Category '{request.Name}' already exists.");

        category.Update(request.Name, request.Description);
        await unitOfWork.SaveChangesAsync(ct);
        return CategoryDto.From(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await GetOrThrowAsync(id, ct);
        if (await categories.HasProductsAsync(id, ct))
            throw new ConflictException("Cannot delete a category that still has products.");

        categories.Remove(category);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Category> GetOrThrowAsync(Guid id, CancellationToken ct)
        => await categories.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Category), id);
}

using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Storage;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Models;
using ECommerce.Domain.Entities;
using FluentValidation;

namespace ECommerce.Application.Features.Products;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductQuery query, CancellationToken ct = default);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> AddStockAsync(Guid id, int quantity, CancellationToken ct = default);
    Task<ProductDto> SetImageAsync(Guid id, UploadImageRequest request, CancellationToken ct = default);
    Task RemoveImageAsync(Guid id, CancellationToken ct = default);
    Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public class ProductService(
    IProductRepository products,
    ICategoryRepository categories,
    IFileStorage fileStorage,
    IValidator<UploadImageRequest> imageValidator,
    IUnitOfWork unitOfWork) : IProductService
{
    private const int MaxPageSize = 100;
    private const string ImageFolder = "products";

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var (items, total) = await products.GetPagedAsync(query.Search, query.CategoryId, page, pageSize, ct);
        return new PagedResult<ProductDto>(items.Select(ToDto).ToList(), page, pageSize, total);
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        => ToDto(await GetOrThrowAsync(id, ct));

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        await EnsureCategoryExistsAsync(request.CategoryId, ct);

        var product = new Product(request.Name, request.Description, request.Price,
            request.StockQuantity, request.CategoryId);

        await products.AddAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return await GetByIdAsync(product.Id, ct);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await GetOrThrowAsync(id, ct);
        await EnsureCategoryExistsAsync(request.CategoryId, ct);

        product.Update(request.Name, request.Description, request.Price, request.CategoryId);
        await unitOfWork.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<ProductDto> AddStockAsync(Guid id, int quantity, CancellationToken ct = default)
    {
        var product = await GetOrThrowAsync(id, ct);
        product.AddStock(quantity);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductDto> SetImageAsync(Guid id, UploadImageRequest request, CancellationToken ct = default)
    {
        await imageValidator.ValidateAndThrowAsync(request, ct);
        var product = await GetOrThrowAsync(id, ct);

        var newPath = await fileStorage.SaveAsync(
            request.Content, ImageFolder, ProductImageRules.GetExtension(request.FileName), ct);
        var oldPath = product.ImagePath;

        try
        {
            product.SetImage(newPath);
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch
        {
            // Don't leave an orphaned file if the database update fails
            await fileStorage.DeleteAsync(newPath, CancellationToken.None);
            throw;
        }

        if (oldPath is not null)
            await fileStorage.DeleteAsync(oldPath, CancellationToken.None);

        return ToDto(product);
    }

    public async Task RemoveImageAsync(Guid id, CancellationToken ct = default)
    {
        var product = await GetOrThrowAsync(id, ct);
        if (product.ImagePath is not { } path)
            return;

        product.RemoveImage();
        await unitOfWork.SaveChangesAsync(ct);
        await fileStorage.DeleteAsync(path, CancellationToken.None);
    }

    public async Task SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var product = await GetOrThrowAsync(id, ct);
        if (isActive) product.Activate();
        else product.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await GetOrThrowAsync(id, ct);
        var imagePath = product.ImagePath;

        products.Remove(product);
        await unitOfWork.SaveChangesAsync(ct);

        if (imagePath is not null)
            await fileStorage.DeleteAsync(imagePath, CancellationToken.None);
    }

    private ProductDto ToDto(Product product)
        => ProductDto.From(product, product.ImagePath is { } path ? fileStorage.GetUrl(path) : null);

    private async Task<Product> GetOrThrowAsync(Guid id, CancellationToken ct)
        => await products.GetWithCategoryAsync(id, ct) ?? throw new NotFoundException(nameof(Product), id);

    private async Task EnsureCategoryExistsAsync(Guid categoryId, CancellationToken ct)
    {
        if (!await categories.ExistsAsync(categoryId, ct))
            throw new NotFoundException(nameof(Category), categoryId);
    }
}

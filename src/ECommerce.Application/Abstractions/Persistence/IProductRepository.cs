using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Persistence;

public interface IProductRepository : IRepository<Product>
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        string? search, Guid? categoryId, int page, int pageSize, CancellationToken ct = default);

    Task<Product?> GetWithCategoryAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}

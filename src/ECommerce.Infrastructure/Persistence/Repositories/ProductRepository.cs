using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class ProductRepository(AppDbContext context) : Repository<Product>(context), IProductRepository
{
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        string? search, Guid? categoryId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{search.Trim()}%"));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Product?> GetWithCategoryAsync(Guid id, CancellationToken ct = default)
        => Set.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        => await Set.Where(p => ids.Contains(p.Id)).ToListAsync(ct);
}

using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class CategoryRepository(AppDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var normalized = name.Trim().ToLower();
        return Set.AnyAsync(c => c.Name.ToLower() == normalized && c.Id != excludeId, ct);
    }

    public Task<bool> HasProductsAsync(Guid id, CancellationToken ct = default)
        => Context.Products.AnyAsync(p => p.CategoryId == id, ct);
}

using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Persistence;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> HasProductsAsync(Guid id, CancellationToken ct = default);
}

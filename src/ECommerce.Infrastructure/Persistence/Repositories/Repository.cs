using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class Repository<T>(AppDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context = context;
    protected DbSet<T> Set => Context.Set<T>();

    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().OrderBy(e => e.CreatedAt).ToListAsync(ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => Set.AnyAsync(e => e.Id == id, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    public void Remove(T entity) => Set.Remove(entity);
}

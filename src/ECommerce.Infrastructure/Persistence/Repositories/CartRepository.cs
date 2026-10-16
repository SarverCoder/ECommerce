using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class CartRepository(AppDbContext context) : Repository<Cart>(context), ICartRepository
{
    public Task<Cart?> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
        => Set.Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);
}

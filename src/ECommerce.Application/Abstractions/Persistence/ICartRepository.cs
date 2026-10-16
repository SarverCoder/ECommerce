using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Persistence;

public interface ICartRepository : IRepository<Cart>
{
    /// <summary>Loads the customer's cart with its items and their products (tracked).</summary>
    Task<Cart?> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
}

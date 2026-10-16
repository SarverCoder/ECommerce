using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Persistence;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
}

using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Persistence;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default);
}

using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class CustomerRepository(AppDbContext context) : Repository<Customer>(context), ICustomerRepository
{
    public Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return Set.AnyAsync(c => c.Email == normalized && c.Id != excludeId, ct);
    }
}

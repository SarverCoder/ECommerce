using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return Set.FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(u => u.RefreshTokenHash == tokenHash, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return Set.AnyAsync(u => u.Email == normalized, ct);
    }
}

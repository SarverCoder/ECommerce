using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Persistence;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}

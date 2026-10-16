using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence;

public static class DbSeeder
{
    /// <summary>Creates the admin account from "Seed:Admin:Email/Password" if it doesn't exist yet.</summary>
    public static async Task SeedAdminAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var email = config["Seed:Admin:Email"];
        var password = config["Seed:Admin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var normalized = email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == normalized, ct))
            return;

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        db.Users.Add(new User(email, hasher.Hash(password), UserRole.Admin));
        await db.SaveChangesAsync(ct);

        scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DbSeeder))
            .LogInformation("Seeded admin user {Email}", normalized);
    }
}

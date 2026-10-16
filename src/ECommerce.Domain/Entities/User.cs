using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }

    // Customer profile linked to this account (null for admins without a profile)
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    // Only the hash of the refresh token is stored
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    private User() { } // EF Core

    public User(string email, string passwordHash, UserRole role, Guid? customerId = null)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("A valid email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        CustomerId = customerId;
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        RevokeRefreshToken();
    }

    public void SetRefreshToken(string tokenHash, DateTime expiresAt)
    {
        RefreshTokenHash = tokenHash;
        RefreshTokenExpiresAt = expiresAt;
        MarkUpdated();
    }

    public bool HasValidRefreshToken(string tokenHash, DateTime now)
        => RefreshTokenHash == tokenHash && RefreshTokenExpiresAt > now;

    public void RevokeRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAt = null;
        MarkUpdated();
    }
}

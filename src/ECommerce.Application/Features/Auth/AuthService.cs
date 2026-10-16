using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Features.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<CurrentUserDto> GetCurrentAsync(CancellationToken ct = default);
}

public class AuthService(
    IUserRepository users,
    ICustomerRepository customers,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await users.EmailExistsAsync(request.Email, ct) || await customers.EmailExistsAsync(request.Email, ct: ct))
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        var customer = new Customer(request.FirstName, request.LastName, request.Email,
            request.PhoneNumber, request.Address);
        var user = new User(request.Email, passwordHasher.Hash(request.Password), UserRole.Customer, customer.Id);

        await customers.AddAsync(customer, ct);
        await users.AddAsync(user, ct);
        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(request.Email, ct);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var user = await users.GetByRefreshTokenHashAsync(hash, ct);
        if (user is null || !user.HasValidRefreshToken(hash, DateTime.UtcNow))
            throw new UnauthorizedException("Invalid or expired refresh token.");

        return await IssueTokensAsync(user, ct); // rotation: old refresh token is replaced
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var user = await GetCurrentUserEntityAsync(ct);
        user.RevokeRefreshToken();
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<CurrentUserDto> GetCurrentAsync(CancellationToken ct = default)
    {
        var user = await GetCurrentUserEntityAsync(ct);
        return new CurrentUserDto(user.Id, user.Email, user.Role.ToString(), user.CustomerId);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        var access = tokenService.CreateAccessToken(user);
        var refresh = tokenService.CreateRefreshToken();
        user.SetRefreshToken(refresh.Hash, refresh.ExpiresAt);
        await unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse(user.Id, user.Email, user.Role.ToString(), user.CustomerId,
            access.Token, access.ExpiresAt, refresh.Token, refresh.ExpiresAt);
    }

    private async Task<User> GetCurrentUserEntityAsync(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId)
            throw new UnauthorizedException("Not authenticated.");
        return await users.GetByIdAsync(userId, ct) ?? throw new UnauthorizedException("User no longer exists.");
    }
}

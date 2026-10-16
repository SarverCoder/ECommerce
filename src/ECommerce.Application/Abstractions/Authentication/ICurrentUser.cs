namespace ECommerce.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    Guid? CustomerId { get; }
    bool IsAdmin { get; }
}

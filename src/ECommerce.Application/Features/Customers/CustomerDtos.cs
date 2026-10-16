using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Customers;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? Address,
    DateTime CreatedAt)
{
    public static CustomerDto From(Customer c) =>
        new(c.Id, c.FirstName, c.LastName, c.Email, c.PhoneNumber, c.Address, c.CreatedAt);
}

public record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? Address);

public record UpdateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? Address);

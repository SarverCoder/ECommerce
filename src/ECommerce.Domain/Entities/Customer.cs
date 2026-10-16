using ECommerce.Domain.Common;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public class Customer : BaseEntity
{
    private readonly List<Order> _orders = [];

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public IReadOnlyCollection<Order> Orders => _orders;

    public string FullName => $"{FirstName} {LastName}";

    private Customer() { } // EF Core

    public Customer(string firstName, string lastName, string email, string? phoneNumber, string? address)
    {
        Update(firstName, lastName, email, phoneNumber, address);
        UpdatedAt = null;
    }

    public void Update(string firstName, string lastName, string email, string? phoneNumber, string? address)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("A valid email is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = phoneNumber;
        Address = address;
        MarkUpdated();
    }
}

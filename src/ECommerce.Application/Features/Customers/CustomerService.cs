using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Customers;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct = default);
    Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public class CustomerService(ICustomerRepository customers, IUnitOfWork unitOfWork) : ICustomerService
{
    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await customers.GetAllAsync(ct);
        return items.Select(CustomerDto.From).ToList();
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        => CustomerDto.From(await GetOrThrowAsync(id, ct));

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        if (await customers.EmailExistsAsync(request.Email, ct: ct))
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        var customer = new Customer(request.FirstName, request.LastName, request.Email,
            request.PhoneNumber, request.Address);

        await customers.AddAsync(customer, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return CustomerDto.From(customer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = await GetOrThrowAsync(id, ct);
        if (await customers.EmailExistsAsync(request.Email, id, ct))
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        customer.Update(request.FirstName, request.LastName, request.Email,
            request.PhoneNumber, request.Address);
        await unitOfWork.SaveChangesAsync(ct);
        return CustomerDto.From(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await GetOrThrowAsync(id, ct);
        customers.Remove(customer);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Customer> GetOrThrowAsync(Guid id, CancellationToken ct)
        => await customers.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Customer), id);
}

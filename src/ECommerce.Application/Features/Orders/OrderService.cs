using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Orders;

public interface IOrderService
{
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetMineAsync(CancellationToken ct = default);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderDto> PayAsync(Guid id, CancellationToken ct = default);
    Task<OrderDto> ShipAsync(Guid id, CancellationToken ct = default);
    Task<OrderDto> DeliverAsync(Guid id, CancellationToken ct = default);
    Task<OrderDto> CancelAsync(Guid id, CancellationToken ct = default);
}

public class OrderService(
    IOrderRepository orders,
    IProductRepository products,
    ICustomerRepository customers,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        => OrderDto.From(await GetOwnedOrThrowAsync(id, ct));

    public async Task<IReadOnlyList<OrderDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        EnsureCanAccessCustomer(customerId);
        if (!await customers.ExistsAsync(customerId, ct))
            throw new NotFoundException(nameof(Customer), customerId);

        var items = await orders.GetByCustomerAsync(customerId, ct);
        return items.Select(OrderDto.From).ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> GetMineAsync(CancellationToken ct = default)
    {
        var customerId = currentUser.CustomerId
            ?? throw new ForbiddenException("Your account has no customer profile.");
        var items = await orders.GetByCustomerAsync(customerId, ct);
        return items.Select(OrderDto.From).ToList();
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var customerId = ResolveCustomerId(request.CustomerId);
        if (!await customers.ExistsAsync(customerId, ct))
            throw new NotFoundException(nameof(Customer), customerId);

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var productMap = (await products.GetByIdsAsync(productIds, ct)).ToDictionary(p => p.Id);

        var missing = productIds.FirstOrDefault(id => !productMap.ContainsKey(id));
        if (missing != Guid.Empty)
            throw new NotFoundException(nameof(Product), missing);

        var order = new Order(customerId, request.ShippingAddress);
        foreach (var item in request.Items)
            order.AddItem(productMap[item.ProductId], item.Quantity);

        await orders.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return OrderDto.From(order);
    }

    public Task<OrderDto> PayAsync(Guid id, CancellationToken ct = default)
        => ChangeAsync(id, o => o.MarkAsPaid(), ct);

    public Task<OrderDto> ShipAsync(Guid id, CancellationToken ct = default)
        => ChangeAsync(id, o => o.Ship(), ct);

    public Task<OrderDto> DeliverAsync(Guid id, CancellationToken ct = default)
        => ChangeAsync(id, o => o.Deliver(), ct);

    public Task<OrderDto> CancelAsync(Guid id, CancellationToken ct = default)
        => ChangeAsync(id, o => o.Cancel(), ct);

    private async Task<OrderDto> ChangeAsync(Guid id, Action<Order> action, CancellationToken ct)
    {
        var order = await GetOwnedOrThrowAsync(id, ct);
        action(order);
        await unitOfWork.SaveChangesAsync(ct);
        return OrderDto.From(order);
    }

    private async Task<Order> GetOwnedOrThrowAsync(Guid id, CancellationToken ct)
    {
        var order = await orders.GetWithItemsAsync(id, ct) ?? throw new NotFoundException(nameof(Order), id);
        // Hide other customers' orders as "not found" rather than revealing they exist
        if (!currentUser.IsAdmin && order.CustomerId != currentUser.CustomerId)
            throw new NotFoundException(nameof(Order), id);
        return order;
    }

    private Guid ResolveCustomerId(Guid? requested)
    {
        if (currentUser.IsAdmin)
            return requested ?? throw new BadRequestException("CustomerId is required when an admin creates an order.");

        var own = currentUser.CustomerId ?? throw new ForbiddenException("Your account has no customer profile.");
        if (requested.HasValue && requested != own)
            throw new ForbiddenException("You can only create orders for yourself.");
        return own;
    }

    private void EnsureCanAccessCustomer(Guid customerId)
    {
        if (!currentUser.IsAdmin && currentUser.CustomerId != customerId)
            throw new ForbiddenException();
    }
}

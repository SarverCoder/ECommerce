using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Orders;

public record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal TotalPrice)
{
    public static OrderItemDto From(OrderItem i) =>
        new(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.TotalPrice);
}

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    string ShippingAddress,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static OrderDto From(Order o) => new(
        o.Id, o.CustomerId, o.Status.ToString(), o.ShippingAddress, o.TotalAmount,
        o.Items.Select(OrderItemDto.From).ToList(), o.CreatedAt, o.UpdatedAt);
}

public record CreateOrderItemRequest(Guid ProductId, int Quantity);

/// <param name="CustomerId">Admin only. Customers always order for their own profile.</param>
public record CreateOrderRequest(
    Guid? CustomerId,
    string ShippingAddress,
    List<CreateOrderItemRequest> Items);

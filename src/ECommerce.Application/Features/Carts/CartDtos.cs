using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Carts;

/// <param name="IsAvailable">False when the product was deactivated or its stock dropped below the quantity.</param>
public record CartItemDto(
    Guid ProductId,
    string ProductName,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice,
    int AvailableStock,
    bool IsAvailable)
{
    public static CartItemDto From(CartItem i, string? imageUrl) => new(
        i.ProductId, i.Product.Name, imageUrl, i.Product.Price, i.Quantity, i.TotalPrice,
        i.Product.StockQuantity, i.Product.IsActive && i.Product.StockQuantity >= i.Quantity);
}

public record CartDto(IReadOnlyList<CartItemDto> Items, int TotalQuantity, decimal TotalAmount, DateTime? UpdatedAt)
{
    public static readonly CartDto Empty = new([], 0, 0, null);
}

public record AddCartItemRequest(Guid ProductId, int Quantity);

public record UpdateCartItemRequest(int Quantity);

/// <param name="ShippingAddress">Optional; defaults to the address in the customer's profile.</param>
public record CheckoutRequest(string? ShippingAddress);

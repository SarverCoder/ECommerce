using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Storage;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Features.Orders;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Features.Carts;

/// <summary>Shopping cart of the signed-in customer; each customer has at most one cart.</summary>
public interface ICartService
{
    Task<CartDto> GetMineAsync(CancellationToken ct = default);
    Task<CartDto> AddItemAsync(AddCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> UpdateItemAsync(Guid productId, UpdateCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> RemoveItemAsync(Guid productId, CancellationToken ct = default);
    Task ClearAsync(CancellationToken ct = default);
    Task<OrderDto> CheckoutAsync(CheckoutRequest request, CancellationToken ct = default);
}

public class CartService(
    ICartRepository carts,
    IProductRepository products,
    ICustomerRepository customers,
    IOrderRepository orders,
    IFileStorage storage,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ICartService
{
    public async Task<CartDto> GetMineAsync(CancellationToken ct = default)
    {
        var cart = await carts.GetByCustomerAsync(CustomerId, ct);
        return cart is null ? CartDto.Empty : ToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(AddCartItemRequest request, CancellationToken ct = default)
    {
        var customerId = CustomerId;
        var product = await products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        // The cart is created lazily on the first added item
        var cart = await carts.GetByCustomerAsync(customerId, ct);
        if (cart is null)
        {
            cart = new Cart(customerId);
            await carts.AddAsync(cart, ct);
        }

        cart.AddItem(product, request.Quantity);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(cart);
    }

    public async Task<CartDto> UpdateItemAsync(Guid productId, UpdateCartItemRequest request, CancellationToken ct = default)
    {
        var cart = await GetCartContainingOrThrowAsync(productId, ct);
        var product = cart.Items.First(i => i.ProductId == productId).Product;

        cart.SetItemQuantity(product, request.Quantity);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(cart);
    }

    public async Task<CartDto> RemoveItemAsync(Guid productId, CancellationToken ct = default)
    {
        var cart = await GetCartContainingOrThrowAsync(productId, ct);

        cart.RemoveItem(productId);
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(cart);
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        var cart = await carts.GetByCustomerAsync(CustomerId, ct);
        if (cart is null || cart.Items.Count == 0)
            return;

        cart.Clear();
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<OrderDto> CheckoutAsync(CheckoutRequest request, CancellationToken ct = default)
    {
        var customerId = CustomerId;
        var cart = await carts.GetByCustomerAsync(customerId, ct);
        if (cart is null || cart.Items.Count == 0)
            throw new BadRequestException("Your cart is empty.");

        var shippingAddress = request.ShippingAddress ?? await GetProfileAddressAsync(customerId, ct);

        // Creating the order, reserving stock and emptying the cart are saved in one transaction
        var order = cart.Checkout(shippingAddress);
        await orders.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return OrderDto.From(order);
    }

    private Guid CustomerId => currentUser.CustomerId
        ?? throw new ForbiddenException("Your account has no customer profile.");

    private async Task<string> GetProfileAddressAsync(Guid customerId, CancellationToken ct)
    {
        var customer = await customers.GetByIdAsync(customerId, ct)
            ?? throw new NotFoundException(nameof(Customer), customerId);

        return string.IsNullOrWhiteSpace(customer.Address)
            ? throw new BadRequestException("Shipping address is required because your profile has no address.")
            : customer.Address;
    }

    private async Task<Cart> GetCartContainingOrThrowAsync(Guid productId, CancellationToken ct)
    {
        var cart = await carts.GetByCustomerAsync(CustomerId, ct);
        if (cart is null || cart.Items.All(i => i.ProductId != productId))
            throw new NotFoundException("Cart item for product", productId);
        return cart;
    }

    private CartDto ToDto(Cart cart)
    {
        var items = cart.Items
            .OrderBy(i => i.CreatedAt)
            .Select(i => CartItemDto.From(i, i.Product.ImagePath is null ? null : storage.GetUrl(i.Product.ImagePath)))
            .ToList();
        return new CartDto(items, items.Sum(i => i.Quantity), cart.TotalAmount, cart.UpdatedAt);
    }
}

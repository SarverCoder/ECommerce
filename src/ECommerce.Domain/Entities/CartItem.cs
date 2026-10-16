using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }

    public decimal TotalPrice => Product.Price * Quantity;

    private CartItem() { } // EF Core

    internal CartItem(Guid cartId, Product product, int quantity)
    {
        CartId = cartId;
        ProductId = product.Id;
        Product = product;
        Quantity = quantity;
    }

    internal void SetQuantity(int quantity)
    {
        Quantity = quantity;
        MarkUpdated();
    }
}

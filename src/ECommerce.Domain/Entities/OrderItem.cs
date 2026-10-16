using ECommerce.Domain.Common;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    // Snapshot of the product at the moment of ordering
    public string ProductName { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public decimal TotalPrice => UnitPrice * Quantity;

    private OrderItem() { } // EF Core

    internal OrderItem(Guid orderId, Product product, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        OrderId = orderId;
        ProductId = product.Id;
        Product = product;
        ProductName = product.Name;
        UnitPrice = product.Price;
        Quantity = quantity;
    }

    internal void IncreaseQuantity(int quantity)
    {
        Quantity += quantity;
        MarkUpdated();
    }
}

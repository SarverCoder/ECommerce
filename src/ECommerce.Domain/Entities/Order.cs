using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = [];

    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public string ShippingAddress { get; private set; } = null!;
    public decimal TotalAmount { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;

    private Order() { } // EF Core

    public Order(Guid customerId, string shippingAddress)
    {
        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new DomainException("Shipping address is required.");

        CustomerId = customerId;
        ShippingAddress = shippingAddress.Trim();
    }

    public void AddItem(Product product, int quantity)
    {
        EnsureStatus(OrderStatus.Pending, "add items to");
        if (!product.IsActive)
            throw new DomainException($"Product '{product.Name}' is not available.");

        product.RemoveStock(quantity);

        var existing = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing is null)
            _items.Add(new OrderItem(Id, product, quantity));
        else
            existing.IncreaseQuantity(quantity);

        TotalAmount = _items.Sum(i => i.TotalPrice);
    }

    public void MarkAsPaid()
    {
        EnsureStatus(OrderStatus.Pending, "pay");
        if (_items.Count == 0)
            throw new DomainException("Cannot pay for an empty order.");
        ChangeStatus(OrderStatus.Paid);
    }

    public void Ship()
    {
        EnsureStatus(OrderStatus.Paid, "ship");
        ChangeStatus(OrderStatus.Shipped);
    }

    public void Deliver()
    {
        EnsureStatus(OrderStatus.Shipped, "deliver");
        ChangeStatus(OrderStatus.Delivered);
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled)
            throw new DomainException($"Cannot cancel an order with status '{Status}'.");

        foreach (var item in _items)
            item.Product.AddStock(item.Quantity);

        ChangeStatus(OrderStatus.Cancelled);
    }

    private void EnsureStatus(OrderStatus expected, string action)
    {
        if (Status != expected)
            throw new DomainException($"Cannot {action} an order with status '{Status}'.");
    }

    private void ChangeStatus(OrderStatus status)
    {
        Status = status;
        MarkUpdated();
    }
}

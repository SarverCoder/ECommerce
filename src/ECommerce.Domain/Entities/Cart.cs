using ECommerce.Domain.Common;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public class Cart : BaseEntity
{
    public const int MaxItems = 50;

    private readonly List<CartItem> _items = [];

    public Guid CustomerId { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items;

    // Calculated from current product prices; prices are fixed only at checkout
    public decimal TotalAmount => _items.Sum(i => i.TotalPrice);

    private Cart() { } // EF Core

    public Cart(Guid customerId)
    {
        CustomerId = customerId;
    }

    public void AddItem(Product product, int quantity)
    {
        var existing = FindItem(product.Id);
        if (existing is not null)
        {
            SetItemQuantity(product, existing.Quantity + quantity);
            return;
        }

        if (_items.Count >= MaxItems)
            throw new DomainException($"Cart cannot contain more than {MaxItems} different products.");

        EnsureCanBuy(product, quantity);
        _items.Add(new CartItem(Id, product, quantity));
        MarkUpdated();
    }

    public void SetItemQuantity(Product product, int quantity)
    {
        var item = FindItem(product.Id)
            ?? throw new DomainException($"Product '{product.Name}' is not in the cart.");

        EnsureCanBuy(product, quantity);
        item.SetQuantity(quantity);
        MarkUpdated();
    }

    public bool RemoveItem(Guid productId)
    {
        var item = FindItem(productId);
        if (item is null)
            return false;

        _items.Remove(item);
        MarkUpdated();
        return true;
    }

    public void Clear()
    {
        _items.Clear();
        MarkUpdated();
    }

    /// <summary>Turns the cart into an order (reserving stock at current prices) and empties the cart.</summary>
    public Order Checkout(string shippingAddress)
    {
        if (_items.Count == 0)
            throw new DomainException("Cannot check out an empty cart.");

        var order = new Order(CustomerId, shippingAddress);
        foreach (var item in _items)
            order.AddItem(item.Product, item.Quantity);

        Clear();
        return order;
    }

    private CartItem? FindItem(Guid productId) => _items.FirstOrDefault(i => i.ProductId == productId);

    private static void EnsureCanBuy(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (!product.IsActive)
            throw new DomainException($"Product '{product.Name}' is not available.");
        if (quantity > product.StockQuantity)
            throw new DomainException($"Not enough stock for '{product.Name}'. Available: {product.StockQuantity}.");
    }
}

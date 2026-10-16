using ECommerce.Domain.Common;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Storage-relative path (e.g. "products/0199....jpg"); the public URL is built by the storage provider
    public string? ImagePath { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    private Product() { } // EF Core

    public Product(string name, string? description, decimal price, int stockQuantity, Guid categoryId)
    {
        Update(name, description, price, categoryId);
        if (stockQuantity < 0)
            throw new DomainException("Stock quantity cannot be negative.");
        StockQuantity = stockQuantity;
        UpdatedAt = null;
    }

    public void Update(string name, string? description, decimal price, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");
        if (price <= 0)
            throw new DomainException("Price must be greater than zero.");

        Name = name.Trim();
        Description = description;
        Price = price;
        CategoryId = categoryId;
        MarkUpdated();
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        StockQuantity += quantity;
        MarkUpdated();
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (quantity > StockQuantity)
            throw new DomainException($"Not enough stock for '{Name}'. Available: {StockQuantity}.");
        StockQuantity -= quantity;
        MarkUpdated();
    }

    public void SetImage(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            throw new DomainException("Image path is required.");
        ImagePath = imagePath;
        MarkUpdated();
    }

    public void RemoveImage()
    {
        ImagePath = null;
        MarkUpdated();
    }

    public void Activate() { IsActive = true; MarkUpdated(); }
    public void Deactivate() { IsActive = false; MarkUpdated(); }
}

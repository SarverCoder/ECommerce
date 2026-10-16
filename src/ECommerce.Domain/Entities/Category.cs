using ECommerce.Domain.Common;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public class Category : BaseEntity
{
    private readonly List<Product> _products = [];

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public IReadOnlyCollection<Product> Products => _products;

    private Category() { } // EF Core

    public Category(string name, string? description)
    {
        SetName(name);
        Description = description;
    }

    public void Update(string name, string? description)
    {
        SetName(name);
        Description = description;
        MarkUpdated();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");
        Name = name.Trim();
    }
}

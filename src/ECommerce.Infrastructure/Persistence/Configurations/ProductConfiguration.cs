using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.ImagePath).HasMaxLength(300);
        builder.HasIndex(p => p.Name);

        // PostgreSQL xmin system column as optimistic concurrency token (protects stock updates)
        builder.Property<uint>("Version").IsRowVersion();
    }
}

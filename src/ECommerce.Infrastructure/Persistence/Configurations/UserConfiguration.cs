using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.Property(u => u.RefreshTokenHash).HasMaxLength(128);
        builder.HasIndex(u => u.RefreshTokenHash).IsUnique();

        builder.HasOne(u => u.Customer)
            .WithOne()
            .HasForeignKey<User>(u => u.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using BikeShop.Api.Domain.Customers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeShop.Api.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(customer => customer.Name).HasMaxLength(100);
        builder.Property(customer => customer.Phone).HasMaxLength(30);
        builder.Property(customer => customer.PhoneDigits).HasMaxLength(20);
        builder.HasIndex(customer => customer.PhoneDigits);
        builder.Property(customer => customer.Email).HasMaxLength(200);
    }
}

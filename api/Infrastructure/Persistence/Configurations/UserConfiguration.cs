using BikeShop.Api.Domain.Users;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeShop.Api.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // NOCASE makes the unique index, and every comparison on this column including the
        // sign-in lookup, treat "Lebis" and "lebis" as the same username.
        builder.Property(user => user.Username).HasMaxLength(50).UseCollation("NOCASE");
        builder.HasIndex(user => user.Username).IsUnique();

        builder.Property(user => user.DisplayName).HasMaxLength(100);
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(20);
    }
}

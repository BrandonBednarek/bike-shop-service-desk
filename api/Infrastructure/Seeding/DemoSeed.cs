using BikeShop.Api.Domain.Users;

namespace BikeShop.Api.Infrastructure.Seeding;

/// <summary>
/// The demo data a reviewer starts with. Demo only: a real shop would create its owner
/// account on first run instead of shipping a known password.
/// </summary>
public static class DemoSeed
{
    public const string Password = "demo-password";

    public static IReadOnlyList<DemoAccount> Accounts { get; } =
    [
        new DemoAccount("owner", "Brandon Bednarek", UserRole.Owner, IsActive: true),
        new DemoAccount("lebis", "Lebis", UserRole.Staff, IsActive: true),
        new DemoAccount("harry", "Harry", UserRole.Staff, IsActive: true),
        new DemoAccount("mack", "Mack", UserRole.Staff, IsActive: false),
    ];
}

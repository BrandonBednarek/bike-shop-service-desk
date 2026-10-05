using BikeShop.Api.Domain.Users;

namespace BikeShop.Api.Infrastructure.Seeding;

public sealed record DemoAccount(string Username, string DisplayName, UserRole Role, bool IsActive);

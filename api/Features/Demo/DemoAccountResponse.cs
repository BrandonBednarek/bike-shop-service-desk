using BikeShop.Api.Domain.Users;

namespace BikeShop.Api.Features.Demo;

public sealed record DemoAccountResponse(string Username, string DisplayName, UserRole Role, bool IsActive, string Password);

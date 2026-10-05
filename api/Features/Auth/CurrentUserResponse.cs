using BikeShop.Api.Domain.Users;

namespace BikeShop.Api.Features.Auth;

public sealed record CurrentUserResponse(int Id, string Username, string DisplayName, UserRole Role);

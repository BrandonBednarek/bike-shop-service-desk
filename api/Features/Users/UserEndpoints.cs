using BikeShop.Api.Infrastructure.Persistence;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BikeShop.Api.Features.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder users = endpoints.MapGroup("/api/users").RequireAuthorization();

        users.MapGet("/", ListUsersAsync);

        return endpoints;
    }

    // Inactive users are included so old labour and notes still show a name; pickers offer only active ones.
    private static async Task<Ok<List<UserResponse>>> ListUsersAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        List<UserResponse> users = await dbContext.Users
            .OrderBy(user => user.DisplayName)
            .Select(user => new UserResponse(user.Id, user.DisplayName, user.IsActive))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(users);
    }
}

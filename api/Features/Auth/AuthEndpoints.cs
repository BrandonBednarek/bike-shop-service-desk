using System.Security.Claims;

using BikeShop.Api.Domain.Users;
using BikeShop.Api.Infrastructure.Persistence;
using BikeShop.Api.Infrastructure.Security;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BikeShop.Api.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder auth = endpoints.MapGroup("/api/auth");

        auth.MapPost("/sign-in", HandleSignInAsync);
        auth.MapPost("/sign-out", HandleSignOut);
        auth.MapGet("/me", ReadCurrentUser).RequireAuthorization();

        return endpoints;
    }

    private static async Task<Results<Ok<CurrentUserResponse>, ProblemHttpResult>> HandleSignInAsync(
        SignInRequest request,
        AppDbContext dbContext,
        IPasswordHashing passwordHashing,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users.SingleOrDefaultAsync(
            candidate => candidate.Username == request.Username,
            cancellationToken);

        if (user is null || !passwordHashing.Matches(user.PasswordHash, request.Password))
            return CreateIncorrectCredentialsProblem();

        if (!user.IsActive)
            return CreateDeactivatedAccountProblem();

        ClaimsPrincipal principal = SessionClaims.CreatePrincipal(user);
        await httpContext.SignInAsync(principal);
        return TypedResults.Ok(SessionClaims.ReadCurrentUser(principal));
    }

    private static SignOutHttpResult HandleSignOut() => TypedResults.SignOut();

    private static Ok<CurrentUserResponse> ReadCurrentUser(ClaimsPrincipal principal) => TypedResults.Ok(SessionClaims.ReadCurrentUser(principal));

    // The same message whether the username or the password is wrong, so the message itself
    // doesn't reveal which usernames exist.
    private static ProblemHttpResult CreateIncorrectCredentialsProblem() => TypedResults.Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "The username or password is incorrect.");

    // Only reached with the correct password, so it reveals nothing to someone guessing.
    private static ProblemHttpResult CreateDeactivatedAccountProblem() => TypedResults.Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "This account has been deactivated.",
            detail: "Ask the owner to reactivate it.");
}

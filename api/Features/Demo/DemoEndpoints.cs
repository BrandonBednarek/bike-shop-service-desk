using BikeShop.Api.Infrastructure.Seeding;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace BikeShop.Api.Features.Demo;

public static class DemoEndpoints
{
    public static IEndpointRouteBuilder MapDemoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/demo-accounts", ListDemoAccounts);
        return endpoints;
    }

    // No sign-in needed, because the sign-in page lists them. Outside demo mode the list is
    // empty, so the page shows no passwords.
    private static Ok<List<DemoAccountResponse>> ListDemoAccounts(IOptions<DemoOptions> demoOptions) =>
        TypedResults.Ok(demoOptions.Value.Enabled ? DemoSeed.Accounts.Select(ToResponse).ToList() : []);

    private static DemoAccountResponse ToResponse(DemoAccount account) =>
        new(account.Username, account.DisplayName, account.Role, account.IsActive, DemoSeed.Password);
}

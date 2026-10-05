using BikeShop.Api.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

namespace BikeShop.Api.Infrastructure.Security;

public static class SecurityExtensions
{
    public static IServiceCollection AddPasswordHashing(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHashing, IdentityPasswordHashing>();
        return services;
    }

    public static IServiceCollection AddCookieSignIn(this IServiceCollection services, AppDataDirectory dataDirectory)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(ConfigureSessionCookie);
        services.AddAuthorization();

        // The session cookie is encrypted with Data Protection keys. Keeping them in the data
        // folder means restarting or rebuilding the container doesn't sign everyone out.
        services.AddDataProtection().PersistKeysToFileSystem(dataDirectory.KeysDirectory);

        return services;
    }

    private static void ConfigureSessionCookie(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = "bikeshop_session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;

        // A fixed lifetime rather than a sliding one: a sliding cookie is reissued while it's
        // in use, so a deactivated user who keeps working would never be signed out.
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;

        options.Events.OnRedirectToLogin = context => RespondWithStatus(context, StatusCodes.Status401Unauthorized);
        options.Events.OnRedirectToAccessDenied = context => RespondWithStatus(context, StatusCodes.Status403Forbidden);
    }

    // The front end calls the API with fetch, which needs a status code, not a redirect to a login page.
    private static Task RespondWithStatus(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        return Task.CompletedTask;
    }
}

using System.Globalization;
using System.Security.Claims;

using BikeShop.Api.Domain.Users;

using Microsoft.AspNetCore.Authentication.Cookies;

namespace BikeShop.Api.Features.Auth;

/// <summary>
/// Converts between a user and the claims stored in their session cookie, so reading the
/// current user needs no database call.
/// </summary>
public static class SessionClaims
{
    private const string DisplayNameClaim = "display_name";

    public static ClaimsPrincipal CreatePrincipal(User user)
    {
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(DisplayNameClaim, user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        ];
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public static CurrentUserResponse ReadCurrentUser(ClaimsPrincipal principal) => new(
            int.Parse(ReadRequiredClaim(principal, ClaimTypes.NameIdentifier), CultureInfo.InvariantCulture),
            ReadRequiredClaim(principal, ClaimTypes.Name),
            ReadRequiredClaim(principal, DisplayNameClaim),
            Enum.Parse<UserRole>(ReadRequiredClaim(principal, ClaimTypes.Role)));

    private static string ReadRequiredClaim(ClaimsPrincipal principal, string claimType) => principal.FindFirstValue(claimType)
            ?? throw new InvalidOperationException($"The session is missing its '{claimType}' claim.");
}

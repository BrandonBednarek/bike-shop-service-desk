using Microsoft.AspNetCore.Identity;

namespace BikeShop.Api.Infrastructure.Security;

/// <summary>
/// Salted PBKDF2 hashing from ASP.NET Core Identity, behind an interface this app owns,
/// so nothing else depends on Identity's user-typed API.
/// </summary>
public sealed class IdentityPasswordHashing : IPasswordHashing
{
    // Identity's hasher accepts a user so custom hashers can vary by user; the default
    // implementation ignores it, so a placeholder object is passed.
    private static readonly object IgnoredUser = new();

    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(IgnoredUser, password);

    public bool Matches(string passwordHash, string password)
    {
        PasswordVerificationResult result = _hasher.VerifyHashedPassword(IgnoredUser, passwordHash, password);
        return result != PasswordVerificationResult.Failed;
    }
}

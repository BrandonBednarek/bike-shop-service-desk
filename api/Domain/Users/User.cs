namespace BikeShop.Api.Domain.Users;

public sealed class User
{
    private User()
    {
    }

    public int Id { get; private set; }

    public string Username { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public static User Create(string username, string displayName, UserRole role, string passwordHash) => new()
    {
        Username = username,
        DisplayName = displayName,
        Role = role,
        PasswordHash = passwordHash,
        IsActive = true,
    };

    public void Deactivate() => IsActive = false;
}

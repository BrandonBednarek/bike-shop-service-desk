using BikeShop.Api.Domain.Users;
using BikeShop.Api.Infrastructure.Persistence;
using BikeShop.Api.Infrastructure.Security;

using Microsoft.EntityFrameworkCore;

namespace BikeShop.Api.Infrastructure.Seeding;

public sealed class DemoUserSeeder(
    AppDbContext dbContext,
    IPasswordHashing passwordHashing,
    ILogger<DemoUserSeeder> logger)
{
    public async Task SeedIfEmptyAsync()
    {
        if (await dbContext.Users.AnyAsync())
            return;

        foreach (DemoAccount account in DemoSeed.Accounts)
            dbContext.Users.Add(CreateUser(account));

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} demo user accounts", DemoSeed.Accounts.Count);
    }

    private User CreateUser(DemoAccount account)
    {
        User user = User.Create(
            account.Username,
            account.DisplayName,
            account.Role,
            passwordHashing.Hash(DemoSeed.Password));

        if (!account.IsActive)
            user.Deactivate();

        return user;
    }
}

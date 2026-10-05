using Microsoft.EntityFrameworkCore;

namespace BikeShop.Api.Infrastructure.Persistence;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, AppDataDirectory dataDirectory) =>
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(dataDirectory.DatabaseConnectionString));

    /// <summary>
    /// Applies any pending migrations. Running migrations at start-up suits a single instance;
    /// with several copies of the app they would run as a separate deployment step instead.
    /// </summary>
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

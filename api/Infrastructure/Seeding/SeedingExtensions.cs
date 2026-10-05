namespace BikeShop.Api.Infrastructure.Seeding;

public static class SeedingExtensions
{
    public static IServiceCollection AddDemoSeeding(this IServiceCollection services)
    {
        services.AddScoped<DemoUserSeeder>();
        return services;
    }

    public static async Task SeedDemoDataAsync(this WebApplication app)
    {
        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
        DemoUserSeeder seeder = scope.ServiceProvider.GetRequiredService<DemoUserSeeder>();
        await seeder.SeedIfEmptyAsync();
    }
}

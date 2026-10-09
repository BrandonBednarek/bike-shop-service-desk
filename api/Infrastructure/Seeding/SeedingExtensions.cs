using Microsoft.Extensions.Options;

namespace BikeShop.Api.Infrastructure.Seeding;

public static class SeedingExtensions
{
    public static IServiceCollection AddDemoSeeding(this IServiceCollection services)
    {
        services.AddOptions<DemoOptions>().BindConfiguration(DemoOptions.SectionName);
        services.AddScoped<DemoUserSeeder>();
        services.AddScoped<DemoJobSeeder>();
        return services;
    }

    public static async Task SeedDemoDataAsync(this WebApplication app)
    {
        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;
        DemoOptions demoOptions = services.GetRequiredService<IOptions<DemoOptions>>().Value;

        // Users always, since nobody could sign in without them; sample jobs only in demo mode.
        await services.GetRequiredService<DemoUserSeeder>().SeedIfEmptyAsync();
        if (demoOptions.Enabled)
            await services.GetRequiredService<DemoJobSeeder>().SeedIfEmptyAsync();
    }
}

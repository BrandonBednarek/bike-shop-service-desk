using System.Net;

using BikeShop.Api.Infrastructure.Seeding;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

using Shouldly;

namespace BikeShop.Api.Tests.TestSupport;

/// <summary>
/// Boots the real API in memory with its own data folder, so each test class runs against
/// the real migrations and seeding in a database no other test class can touch.
/// </summary>
public sealed class BikeShopApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dataDirectory =
        Path.Combine(Path.GetTempPath(), "bikeshop-tests", Guid.NewGuid().ToString("N"));

    public async Task<HttpClient> CreateSignedInClientAsync(string username)
    {
        HttpClient client = CreateClient();
        HttpResponseMessage response = await client.SignInAsAsync(username, DemoSeed.Password);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, $"signing in as '{username}' should succeed");
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Storage:DataDirectory", _dataDirectory);
        builder.UseWebRoot(CreateStandInWebRoot());
    }

    // The built front end isn't part of the test run, so a minimal fallback page stands in
    // for it, letting tests check which requests are answered with the app.
    private string CreateStandInWebRoot()
    {
        string webRoot = Path.Combine(_dataDirectory, "wwwroot");
        Directory.CreateDirectory(webRoot);
        File.WriteAllText(Path.Combine(webRoot, "200.html"), "<!doctype html><title>App</title>");
        return webRoot;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        DeleteDataDirectory();
    }

    private void DeleteDataDirectory()
    {
        // Pooled SQLite connections keep the database file open, and Windows won't delete open files.
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_dataDirectory))
            Directory.Delete(_dataDirectory, recursive: true);
    }
}

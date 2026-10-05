using Microsoft.Data.Sqlite;

namespace BikeShop.Api.Infrastructure.Persistence;

/// <summary>
/// The one folder holding everything the app must keep between restarts. Set with
/// <c>Storage:DataDirectory</c>: relative to the app locally, a Docker volume in the
/// container, and a temporary folder per test class in integration tests.
/// </summary>
public sealed class AppDataDirectory(IConfiguration configuration, IHostEnvironment environment)
{
    private const string DefaultDirectory = ".data";

    private readonly string _fullPath = Path.GetFullPath(
        Path.Combine(environment.ContentRootPath, configuration["Storage:DataDirectory"] ?? DefaultDirectory));

    public string DatabaseConnectionString =>
        new SqliteConnectionStringBuilder { DataSource = Path.Combine(_fullPath, "bikeshop.db") }.ToString();

    public DirectoryInfo KeysDirectory => new(Path.Combine(_fullPath, "keys"));

    public void EnsureExists() => Directory.CreateDirectory(_fullPath);
}

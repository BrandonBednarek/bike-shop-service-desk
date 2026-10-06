using System.Text.Json.Serialization;

using BikeShop.Api.Features.Auth;
using BikeShop.Api.Features.Customers;
using BikeShop.Api.Features.Users;
using BikeShop.Api.Infrastructure.Errors;
using BikeShop.Api.Infrastructure.Persistence;
using BikeShop.Api.Infrastructure.Security;
using BikeShop.Api.Infrastructure.Seeding;

// Configuration
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
AppDataDirectory dataDirectory = new(builder.Configuration, builder.Environment);
dataDirectory.EnsureExists();

// Services
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetailsErrors();
builder.Services.AddPersistence(dataDirectory);
builder.Services.AddPasswordHashing();
builder.Services.AddCookieSignIn(dataDirectory);
builder.Services.AddDemoSeeding();

WebApplication app = builder.Build();

// Database
await app.MigrateDatabaseAsync();
await app.SeedDemoDataAsync();

// Request pipeline
app.UseProblemDetailsErrors();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapAuthEndpoints();
app.MapCustomerEndpoints();
app.MapUserEndpoints();
app.MapApiNotFoundFallback();

// Single-page app: any other address that isn't a file gets the front end's page.
app.MapFallbackToFile("200.html");

await app.RunAsync();

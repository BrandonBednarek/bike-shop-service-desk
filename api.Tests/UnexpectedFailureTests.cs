using System.Net;

using BikeShop.Api.Infrastructure.Security;
using BikeShop.Api.Infrastructure.Seeding;
using BikeShop.Api.Tests.TestSupport;

using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace BikeShop.Api.Tests;

public sealed class UnexpectedFailureTests(BikeShopApiFactory factory) : IClassFixture<BikeShopApiFactory>
{
    private const string InternalDetail = "internal detail that must not reach the user";

    [Fact]
    public async Task UnexpectedFailureReturnsAProblemResponseWithoutInternalDetails()
    {
        HttpClient client = CreateClientWhosePasswordCheckThrows();

        HttpResponseMessage response = await client.SignInAsAsync("lebis", DemoSeed.Password);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.ShouldHaveMediaType("application/problem+json");
        string body = await response.Content.ReadAsStringAsync();
        body.ShouldNotContain(InternalDetail);
    }

    private HttpClient CreateClientWhosePasswordCheckThrows() => factory
        .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddSingleton<IPasswordHashing, ThrowingPasswordHashing>()))
        .CreateClient();

    private sealed class ThrowingPasswordHashing : IPasswordHashing
    {
        private readonly IdentityPasswordHashing _realHashing = new();

        // Start-up seeding hashes the demo password through this same service, so hashing still works.
        public string Hash(string password) => _realHashing.Hash(password);

        public bool Matches(string passwordHash, string password) => throw new InvalidOperationException(InternalDetail);
    }
}

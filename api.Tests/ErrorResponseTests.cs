using System.Net;
using System.Text;

using BikeShop.Api.Tests.TestSupport;

using Microsoft.AspNetCore.Http;

using Shouldly;

namespace BikeShop.Api.Tests;

public sealed class ErrorResponseTests(BikeShopApiFactory factory) : IClassFixture<BikeShopApiFactory>
{
    private const string ProblemJson = "application/problem+json";

    [Fact]
    public async Task UnknownApiAddressReturnsProblemDetailsRatherThanTheApp()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/no-such-endpoint");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.ShouldHaveMediaType(ProblemJson);
    }

    [Fact]
    public async Task PageAddressesStillLoadTheAppWhenSignedOut()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/jobs/1004");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.ShouldHaveMediaType("text/html");
    }

    [Fact]
    public async Task MalformedJsonIsABadRequestWithProblemDetails()
    {
        HttpClient client = factory.CreateClient();
        using StringContent brokenJson = new("{", Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/api/auth/sign-in", brokenJson);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ShouldHaveMediaType(ProblemJson);
    }

    [Fact]
    public async Task ErrorsWithoutABodyStillReturnProblemDetails()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/auth/me");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.ShouldHaveMediaType(ProblemJson);
    }

    // .NET 10's Minimal API validation silently does nothing unless AddValidation() is wired up.
    [Fact]
    public async Task MissingRequiredFieldsAreReportedPerField()
    {
        HttpClient client = factory.CreateClient();
        using StringContent emptyObject = new("{}", Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/api/auth/sign-in", emptyObject);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        HttpValidationProblemDetails problem = await response.ReadJsonAsync<HttpValidationProblemDetails>();
        problem.Errors.Keys.ShouldBe(["Username", "Password"], ignoreOrder: true);
    }
}

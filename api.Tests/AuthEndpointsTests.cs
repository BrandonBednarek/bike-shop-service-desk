using System.Net;

using BikeShop.Api.Domain.Users;
using BikeShop.Api.Features.Auth;
using BikeShop.Api.Infrastructure.Seeding;
using BikeShop.Api.Tests.TestSupport;

using Microsoft.AspNetCore.Mvc;

using Shouldly;

namespace BikeShop.Api.Tests;

public sealed class AuthEndpointsTests(BikeShopApiFactory factory) : IClassFixture<BikeShopApiFactory>
{
    [Fact]
    public async Task SigningInWithTheRightPasswordReturnsTheSignedInUser()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.SignInAsAsync("lebis", DemoSeed.Password);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        CurrentUserResponse user = await response.ReadJsonAsync<CurrentUserResponse>();
        user.DisplayName.ShouldBe("Lebis");
    }

    [Fact]
    public async Task SigningInIssuesAnHttpOnlySameSiteStrictSessionCookie()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.SignInAsAsync("lebis", DemoSeed.Password);

        string sessionCookie = response.Headers.GetValues("Set-Cookie")
            .Single(cookie => cookie.StartsWith("bikeshop_session=", StringComparison.Ordinal));
        sessionCookie.ShouldContain("httponly", Case.Insensitive);
        sessionCookie.ShouldContain("samesite=strict", Case.Insensitive);
    }

    [Fact]
    public async Task UsernamesAreNotCaseSensitive()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.SignInAsAsync("LEBIS", DemoSeed.Password);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("lebis", "not-the-password")]
    [InlineData("nobody", DemoSeed.Password)]
    public async Task AWrongUsernameOrPasswordGetsTheSameGenericRejection(string username, string password)
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.SignInAsAsync(username, password);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        ProblemDetails problem = await response.ReadJsonAsync<ProblemDetails>();
        problem.Title.ShouldBe("The username or password is incorrect.");
    }

    [Fact]
    public async Task DeactivatedUserCannotSignInEvenWithTheRightPassword()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.SignInAsAsync("mack", DemoSeed.Password);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReadingTheCurrentUserRequiresSigningInFirst()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/auth/me");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignedInUserCanReadTheirOwnDetails()
    {
        HttpClient client = await factory.CreateSignedInClientAsync("owner");

        HttpResponseMessage response = await client.GetAsync("/api/auth/me");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        CurrentUserResponse user = await response.ReadJsonAsync<CurrentUserResponse>();
        (user.Username, user.Role).ShouldBe(("owner", UserRole.Owner));
    }

    [Fact]
    public async Task SigningOutEndsTheSession()
    {
        HttpClient client = await factory.CreateSignedInClientAsync("lebis");
        HttpResponseMessage beforeSignOut = await client.GetAsync("/api/auth/me");
        beforeSignOut.StatusCode.ShouldBe(HttpStatusCode.OK, "the session should exist before signing out");

        HttpResponseMessage signOutResponse = await client.PostAsync("/api/auth/sign-out", content: null);
        HttpResponseMessage afterSignOut = await client.GetAsync("/api/auth/me");

        signOutResponse.IsSuccessStatusCode.ShouldBeTrue();
        afterSignOut.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}

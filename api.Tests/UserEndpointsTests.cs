using System.Net;

using BikeShop.Api.Features.Users;
using BikeShop.Api.Tests.TestSupport;

using Shouldly;

namespace BikeShop.Api.Tests;

public sealed class UserEndpointsTests(BikeShopApiFactory factory) : IClassFixture<BikeShopApiFactory>
{
    [Fact]
    public async Task StaffCanListEveryoneByNameIncludingInactiveUsers()
    {
        HttpClient client = await factory.CreateSignedInClientAsync("lebis");

        HttpResponseMessage response = await client.GetAsync("/api/users");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<UserResponse> users = await response.ReadJsonAsync<List<UserResponse>>();
        users.ShouldSatisfyAllConditions(
            () => users.Select(user => user.DisplayName).ShouldBe(["Brandon Bednarek", "Harry", "Lebis", "Mack"]),
            () => users.Single(user => user.DisplayName == "Mack").IsActive.ShouldBeFalse());
    }
}

using System.Net;
using System.Net.Http.Json;

using BikeShop.Api.Features.Customers;
using BikeShop.Api.Tests.TestSupport;

using Shouldly;

namespace BikeShop.Api.Tests;

public sealed class CustomerEndpointsTests(BikeShopApiFactory factory) : IClassFixture<BikeShopApiFactory>
{
    [Fact]
    public async Task SearchFindsACustomerByPartOfTheirNameOrByTheirPhoneWrittenAnotherWay()
    {
        HttpClient client = await factory.CreateSignedInClientAsync("lebis");
        HttpResponseMessage created = await client.PostAsJsonAsync("/api/customers", new { name = "Henry Fonda", phone = "(416) 123-4567" });
        created.StatusCode.ShouldBe(HttpStatusCode.OK);

        List<CustomerResponse> byName = await SearchAsync(client, "fonda");
        List<CustomerResponse> byPhone = await SearchAsync(client, "416.123.4567");

        byName.ShouldContain(customer => customer.Name == "Henry Fonda");
        byPhone.ShouldContain(customer => customer.Name == "Henry Fonda");
    }

    [Fact]
    public async Task CustomerSearchNeedsASignedInUser()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/customers?search=fonda");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static async Task<List<CustomerResponse>> SearchAsync(HttpClient client, string search)
    {
        HttpResponseMessage response = await client.GetAsync($"/api/customers?search={Uri.EscapeDataString(search)}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.ReadJsonAsync<List<CustomerResponse>>();
    }
}

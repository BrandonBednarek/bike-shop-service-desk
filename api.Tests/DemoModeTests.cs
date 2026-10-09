using System.Net;

using BikeShop.Api.Domain.WorkOrders;
using BikeShop.Api.Features.Demo;
using BikeShop.Api.Features.WorkOrders;
using BikeShop.Api.Infrastructure.Seeding;
using BikeShop.Api.Tests.TestSupport;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using Shouldly;

namespace BikeShop.Api.Tests;

// Demo mode is off unless it's turned on, so every other test class starts with an empty shop.
// Tests that need demo mode start their own copy of the API with it on, as Docker Compose runs
// it, sharing this class's test database.
public sealed class DemoModeTests(BikeShopApiFactory factory) : IClassFixture<BikeShopApiFactory>
{
    private readonly WebApplicationFactory<Program> _demoModeFactory =
        factory.WithWebHostBuilder(builder => builder.UseSetting("Demo:Enabled", "true"));

    [Fact]
    public async Task SignInPageListsEveryDemoAccountInDemoMode()
    {
        HttpClient client = _demoModeFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/demo-accounts");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<DemoAccountResponse> accounts = await response.ReadJsonAsync<List<DemoAccountResponse>>();
        accounts.Select(account => account.Username).ShouldBe(["owner", "lebis", "harry", "mack"]);
    }

    [Fact]
    public async Task SignInPageListsNoAccountsOutsideDemoMode()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/demo-accounts");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<DemoAccountResponse> accounts = await response.ReadJsonAsync<List<DemoAccountResponse>>();
        accounts.ShouldBeEmpty();
    }

    [Fact]
    public async Task DemoJobsFillTheBoardWithEveryOpenStatus()
    {
        HttpClient client = _demoModeFactory.CreateClient();
        await client.SignInAsAsync("lebis", DemoSeed.Password).ShouldSucceedAsync();

        HttpResponseMessage response = await client.GetAsync("/api/work-orders");

        List<WorkOrderResponse> workOrders = await response.ReadJsonAsync<List<WorkOrderResponse>>();
        workOrders.Select(workOrder => workOrder.Status).Distinct().ShouldBe(
            [WorkOrderStatus.CheckedIn, WorkOrderStatus.InProgress, WorkOrderStatus.OnHold, WorkOrderStatus.ReadyForPickup],
            ignoreOrder: true);
    }
}

using System.Net;

using BikeShop.Api.Domain.WorkOrders;
using BikeShop.Api.Features.WorkOrders;
using BikeShop.Api.Tests.TestSupport;

using Microsoft.AspNetCore.Mvc;

using Shouldly;

namespace BikeShop.Api.Tests;

public sealed class WorkOrderEndpointsTests(BikeShopApiFactory factory) : IClassFixture<BikeShopApiFactory>
{
    [Fact]
    public async Task ACheckedInJobCanBeFetchedByItsJobNumber()
    {
        HttpClient client = await factory.CreateSignedInClientAsync("lebis");
        WorkOrderResponse checkedIn = await client.CheckInTuneUpAsync();

        HttpResponseMessage response = await client.GetAsync($"/api/work-orders/{checkedIn.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        WorkOrderResponse fetched = await response.ReadJsonAsync<WorkOrderResponse>();
        fetched.ShouldSatisfyAllConditions(
            () => fetched.Id.ShouldBeGreaterThanOrEqualTo(1001),
            () => fetched.CustomerName.ShouldBe("Henry Fonda"),
            () => fetched.Status.ShouldBe(WorkOrderStatus.CheckedIn),
            () => fetched.EstimateTotalCents.ShouldBe(16_075),
            () => fetched.CheckedInAtUtc.Kind.ShouldBe(DateTimeKind.Utc));
    }

    [Fact]
    public async Task ACheckedInJobIsOnTheBoard()
    {
        HttpClient client = await factory.CreateSignedInClientAsync("lebis");
        WorkOrderResponse checkedIn = await client.CheckInTuneUpAsync();

        HttpResponseMessage response = await client.GetAsync("/api/work-orders");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<WorkOrderResponse> board = await response.ReadJsonAsync<List<WorkOrderResponse>>();
        board.ShouldContain(workOrder => workOrder.Id == checkedIn.Id);
    }

    [Fact]
    public async Task AnUnknownJobNumberIsNotFound()
    {
        HttpClient client = await factory.CreateSignedInClientAsync("lebis");

        HttpResponseMessage response = await client.GetAsync("/api/work-orders/999999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails problem = await response.ReadJsonAsync<ProblemDetails>();
        problem.Title.ShouldBe("There's no job with that number.");
    }
}

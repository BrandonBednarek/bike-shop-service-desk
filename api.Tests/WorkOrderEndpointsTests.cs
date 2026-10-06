using System.Net;
using System.Net.Http.Json;

using BikeShop.Api.Domain.WorkOrders;
using BikeShop.Api.Features.Customers;
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
        WorkOrderResponse checkedIn = await CheckInTuneUpAsync(client);

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
        WorkOrderResponse checkedIn = await CheckInTuneUpAsync(client);

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

    private static async Task<WorkOrderResponse> CheckInTuneUpAsync(HttpClient client)
    {
        HttpResponseMessage customerResponse = await client.PostAsJsonAsync("/api/customers", new { name = "Henry Fonda", phone = "(416) 123-4567" });
        CustomerResponse customer = await customerResponse.ReadJsonAsync<CustomerResponse>();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/work-orders", new
        {
            customerId = customer.Id,
            bikeMakeModel = "Trek FX 2",
            bikeColour = "Matte black",
            jobType = "TuneUp",
            workRequested = "Gears skip on the big cog and the brakes squeal.",
            estimatedLabourMinutes = 75,
            labourRateCentsPerHour = 9_500,
            estimatedPartsCents = 4_200,
            promisedOn = "2026-10-09",
        });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.ReadJsonAsync<WorkOrderResponse>();
    }
}

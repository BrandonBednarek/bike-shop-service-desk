using System.Net;
using System.Net.Http.Json;

using BikeShop.Api.Features.Customers;
using BikeShop.Api.Features.WorkOrders;

using Shouldly;

namespace BikeShop.Api.Tests.TestSupport;

public static class WorkOrderApiExtensions
{
    public static async Task<WorkOrderResponse> CheckInTuneUpAsync(this HttpClient client)
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

    public static Task<HttpResponseMessage> PostActionAsync(this HttpClient client, int workOrderId, string action) =>
        client.PostAsync($"/api/work-orders/{workOrderId}/{action}", content: null);

    public static Task<HttpResponseMessage> PostActionAsync<TBody>(this HttpClient client, int workOrderId, string action, TBody body) =>
        client.PostAsJsonAsync($"/api/work-orders/{workOrderId}/{action}", body);
}

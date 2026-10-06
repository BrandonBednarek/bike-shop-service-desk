using System.Net;

using BikeShop.Api.Domain.WorkOrders;
using BikeShop.Api.Features.WorkOrders;
using BikeShop.Api.Tests.TestSupport;

using Shouldly;

namespace BikeShop.Api.Tests;

public sealed class WorkOrderStatusEndpointsTests(BikeShopApiFactory factory) : IClassFixture<BikeShopApiFactory>
{
    [Fact]
    public async Task AJobGoesFromCheckInToCollected()
    {
        HttpClient client = await factory.CreateSignedInClientAsync("lebis");
        WorkOrderResponse job = await client.CheckInTuneUpAsync();

        await ShouldSucceedAsync(client.PostActionAsync(job.Id, "start"));
        await ShouldSucceedAsync(client.PostActionAsync(job.Id, "hold", new { reason = "WaitingForParts", note = "Cassette due Thursday." }));
        await ShouldSucceedAsync(client.PostActionAsync(job.Id, "start"));
        await ShouldSucceedAsync(client.PostActionAsync(job.Id, "mark-ready"));
        HttpResponseMessage collected = await client.PostActionAsync(job.Id, "collect", new { posReceiptNumber = "R-1042" });

        WorkOrderResponse final = await collected.ReadJsonAsync<WorkOrderResponse>();
        final.ShouldSatisfyAllConditions(
            () => final.Status.ShouldBe(WorkOrderStatus.Collected),
            () => final.PosReceiptNumber.ShouldBe("R-1042"),
            () => final.Notes.ShouldHaveSingleItem().Text.ShouldBe("Cassette due Thursday."));
    }

    [Fact]
    public async Task AMoveTheLifecycleDoesNotAllowIsUnprocessable()
    {
        HttpClient client = await factory.CreateSignedInClientAsync("lebis");
        WorkOrderResponse job = await client.CheckInTuneUpAsync();

        HttpResponseMessage response = await client.PostActionAsync(job.Id, "collect", new { posReceiptNumber = "R-1042" });

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        response.ShouldHaveMediaType("application/problem+json");
    }

    [Fact]
    public async Task OnlyTheOwnerCanReopenAJob()
    {
        HttpClient staff = await factory.CreateSignedInClientAsync("lebis");
        HttpClient owner = await factory.CreateSignedInClientAsync("owner");
        WorkOrderResponse job = await staff.CheckInTuneUpAsync();
        await ShouldSucceedAsync(staff.PostActionAsync(job.Id, "cancel", new { reason = "Customer took the bike home." }));

        HttpResponseMessage byStaff = await staff.PostActionAsync(job.Id, "reopen");
        HttpResponseMessage byOwner = await owner.PostActionAsync(job.Id, "reopen");

        byStaff.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        byOwner.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await byOwner.ReadJsonAsync<WorkOrderResponse>()).Status.ShouldBe(WorkOrderStatus.CheckedIn);
    }

    private static async Task ShouldSucceedAsync(Task<HttpResponseMessage> request) =>
        (await request).StatusCode.ShouldBe(HttpStatusCode.OK);
}

using System.Net;
using System.Net.Http.Json;

using BikeShop.Api.Features.WorkOrders;
using BikeShop.Api.Tests.TestSupport;

using Shouldly;

namespace BikeShop.Api.Tests;

public sealed class WorkOrderLineEndpointsTests(BikeShopApiFactory factory) : IClassFixture<BikeShopApiFactory>
{
    [Fact]
    public async Task LabourPartsAndNotesAreAddedToTheJob()
    {
        HttpClient client = await factory.CreateSignedInClientAsync("lebis");
        WorkOrderResponse job = await client.CheckInTuneUpAsync();

        await client.PostAsJsonAsync($"/api/work-orders/{job.Id}/labour", new { minutes = 120, note = "Seized derailleur bolt." });
        await client.PostAsJsonAsync($"/api/work-orders/{job.Id}/parts", new { description = "Brake pads", quantity = 2, unitPriceCents = 1_500 });
        HttpResponseMessage response = await client.PostAsJsonAsync($"/api/work-orders/{job.Id}/notes", new { text = "Customer called; picking up Friday." });

        WorkOrderResponse updated = await response.ReadJsonAsync<WorkOrderResponse>();
        updated.ShouldSatisfyAllConditions(
            () => updated.BillCents.ShouldBe(14_875),
            () => updated.LoggedLabourMinutes.ShouldBe(120),
            () => updated.Notes.ShouldHaveSingleItem().Text.ShouldBe("Customer called; picking up Friday."));
    }

    [Fact]
    public async Task RemovingAPartTakesItOffTheBill()
    {
        HttpClient client = await factory.CreateSignedInClientAsync("lebis");
        WorkOrderResponse job = await client.CheckInTuneUpAsync();
        HttpResponseMessage added = await client.PostAsJsonAsync($"/api/work-orders/{job.Id}/parts", new { description = "Chain", quantity = 1, unitPriceCents = 5_000 });
        int partId = (await added.ReadJsonAsync<WorkOrderResponse>()).PartLines.Single().Id;

        HttpResponseMessage response = await client.DeleteAsync($"/api/work-orders/{job.Id}/parts/{partId}");

        WorkOrderResponse updated = await response.ReadJsonAsync<WorkOrderResponse>();
        updated.ShouldSatisfyAllConditions(
            () => updated.PartLines.ShouldBeEmpty(),
            () => updated.BillCents.ShouldBe(11_875));
    }

    [Fact]
    public async Task StaffCannotChangeAClosedJobButTheOwnerCan()
    {
        HttpClient staff = await factory.CreateSignedInClientAsync("lebis");
        HttpClient owner = await factory.CreateSignedInClientAsync("owner");
        WorkOrderResponse job = await staff.CheckInTuneUpAsync();
        (await staff.PostActionAsync(job.Id, "cancel", new { reason = "Customer took the bike home." })).StatusCode.ShouldBe(HttpStatusCode.OK);
        object part = new { description = "Brake pads", quantity = 2, unitPriceCents = 1_500 };

        HttpResponseMessage byStaff = await staff.PostAsJsonAsync($"/api/work-orders/{job.Id}/parts", part);
        HttpResponseMessage byOwner = await owner.PostAsJsonAsync($"/api/work-orders/{job.Id}/parts", part);

        byStaff.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        byOwner.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

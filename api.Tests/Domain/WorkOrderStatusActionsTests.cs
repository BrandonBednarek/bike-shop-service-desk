using BikeShop.Api.Domain;
using BikeShop.Api.Domain.WorkOrders;

using Shouldly;

namespace BikeShop.Api.Tests.Domain;

public sealed class WorkOrderStatusActionsTests
{
    private const int CounterStaffUserId = 2;

    private const int MechanicUserId = 3;

    private static readonly DateTime CheckedInAtUtc = new(2026, 10, 5, 14, 30, 0, DateTimeKind.Utc);

    private static readonly DateTime LaterUtc = new(2026, 10, 6, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void AMoveTheLifecycleDoesNotAllowIsRefused()
    {
        WorkOrder workOrder = CheckInUnassignedTuneUp();

        Should.Throw<BusinessRuleException>(() => workOrder.Collect("R-1042", LaterUtc));
        workOrder.Status.ShouldBe(WorkOrderStatus.CheckedIn);
    }

    [Fact]
    public void StartingAnUnassignedJobAssignsItToWhoeverStartedIt()
    {
        WorkOrder workOrder = CheckInUnassignedTuneUp();

        workOrder.Start(MechanicUserId, LaterUtc);

        workOrder.ShouldSatisfyAllConditions(
            () => workOrder.Status.ShouldBe(WorkOrderStatus.InProgress),
            () => workOrder.AssignedToUserId.ShouldBe(MechanicUserId),
            () => workOrder.StatusChangedAtUtc.ShouldBe(LaterUtc));
    }

    [Fact]
    public void ReopeningACollectedJobPutsItBackAsReadyAndClearsTheReceipt()
    {
        WorkOrder workOrder = CheckInUnassignedTuneUp();
        workOrder.Start(MechanicUserId, LaterUtc);
        workOrder.MarkReady(LaterUtc);
        workOrder.Collect("R-1042", LaterUtc);

        workOrder.Reopen(LaterUtc);

        workOrder.ShouldSatisfyAllConditions(
            () => workOrder.Status.ShouldBe(WorkOrderStatus.ReadyForPickup),
            () => workOrder.PosReceiptNumber.ShouldBeNull());
    }

    [Fact]
    public void ReopeningACancelledJobPutsItBackAsCheckedInAndClearsTheReason()
    {
        WorkOrder workOrder = CheckInUnassignedTuneUp();
        workOrder.Cancel("Customer took the bike home.", LaterUtc);

        workOrder.Reopen(LaterUtc);

        workOrder.ShouldSatisfyAllConditions(
            () => workOrder.Status.ShouldBe(WorkOrderStatus.CheckedIn),
            () => workOrder.CancellationReason.ShouldBeNull());
    }

    [Fact]
    public void NotesAreKeptWhenTheStatusChanges()
    {
        WorkOrder workOrder = CheckInUnassignedTuneUp();

        workOrder.AddNote("Waiting for a Shimano 11-speed cassette, due Thursday.", MechanicUserId, LaterUtc);
        workOrder.Hold(HoldReason.WaitingForParts, LaterUtc);
        workOrder.Start(MechanicUserId, LaterUtc);
        workOrder.AddNote("Customer called; picking up Friday.", CounterStaffUserId, LaterUtc);

        workOrder.Notes.Select(note => note.Text).ShouldBe(
        [
            "Waiting for a Shimano 11-speed cassette, due Thursday.",
            "Customer called; picking up Friday.",
        ]);
    }

    private static WorkOrder CheckInUnassignedTuneUp() => WorkOrder.CheckIn(
        new WorkOrderIntake(
            CustomerId: 7,
            BikeMakeModel: "Trek FX 2",
            BikeColour: "Matte black",
            JobType: JobType.TuneUp,
            WorkRequested: "Annual tune-up.",
            EstimatedLabourMinutes: 75,
            LabourRateCentsPerHour: 9_500,
            EstimatedPartsCents: 0,
            PromisedOn: new DateOnly(2026, 10, 9),
            AssignedToUserId: null),
        CounterStaffUserId,
        CheckedInAtUtc);
}

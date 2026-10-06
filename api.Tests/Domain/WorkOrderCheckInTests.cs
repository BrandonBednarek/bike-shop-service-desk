using BikeShop.Api.Domain.WorkOrders;

using Shouldly;

namespace BikeShop.Api.Tests.Domain;

public sealed class WorkOrderCheckInTests
{
    private const int CounterStaffUserId = 2;

    private static readonly DateTime CheckedInAtUtc = new(2026, 10, 5, 14, 30, 0, DateTimeKind.Utc);

    private static readonly WorkOrderIntake TuneUpIntake = new(
        CustomerId: 7,
        BikeMakeModel: "Trek FX 2",
        BikeColour: "Matte black",
        JobType: JobType.TuneUp,
        WorkRequested: "Gears skip on the big cog and the brakes squeal.",
        EstimatedLabourMinutes: 75,
        LabourRateCentsPerHour: 9_500,
        EstimatedPartsCents: 4_200,
        PromisedOn: new DateOnly(2026, 10, 9),
        AssignedToUserId: 3);

    [Fact]
    public void CheckingInRecordsTheIntakeAndStartsAsCheckedIn()
    {
        WorkOrder workOrder = CheckIn(TuneUpIntake);

        workOrder.ShouldSatisfyAllConditions(
            () => workOrder.Status.ShouldBe(WorkOrderStatus.CheckedIn),
            () => workOrder.CustomerId.ShouldBe(7),
            () => workOrder.BikeMakeModel.ShouldBe("Trek FX 2"),
            () => workOrder.BikeColour.ShouldBe("Matte black"),
            () => workOrder.JobType.ShouldBe(JobType.TuneUp),
            () => workOrder.WorkRequested.ShouldBe("Gears skip on the big cog and the brakes squeal."),
            () => workOrder.EstimatedLabourMinutes.ShouldBe(75),
            () => workOrder.LabourRateCentsPerHour.ShouldBe(9_500),
            () => workOrder.EstimatedPartsCents.ShouldBe(4_200),
            () => workOrder.PromisedOn.ShouldBe(new DateOnly(2026, 10, 9)),
            () => workOrder.AssignedToUserId.ShouldBe(3),
            () => workOrder.CheckedInByUserId.ShouldBe(CounterStaffUserId),
            () => workOrder.CheckedInAtUtc.ShouldBe(CheckedInAtUtc),
            () => workOrder.StatusChangedAtUtc.ShouldBe(CheckedInAtUtc));
    }

    [Fact]
    public void TheEstimateIsLabourAtTheJobsRatePlusPartsRoundedToTheCent()
    {
        // 45 minutes at $85.50 an hour is $64.125, so this also checks that half a cent rounds up.
        WorkOrder workOrder = CheckIn(TuneUpIntake with
        {
            EstimatedLabourMinutes = 45,
            LabourRateCentsPerHour = 8_550,
            EstimatedPartsCents = 1_200,
        });

        workOrder.EstimateTotalCents.ShouldBe(7_613);
    }

    private static WorkOrder CheckIn(WorkOrderIntake intake) => WorkOrder.CheckIn(intake, CounterStaffUserId, CheckedInAtUtc);
}

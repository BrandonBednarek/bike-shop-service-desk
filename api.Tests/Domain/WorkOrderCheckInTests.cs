using BikeShop.Api.Domain.WorkOrders;

using Shouldly;

using static BikeShop.Api.Tests.Domain.TuneUpJob;

namespace BikeShop.Api.Tests.Domain;

public sealed class WorkOrderCheckInTests
{
    [Fact]
    public void CheckingInRecordsTheIntakeAndStartsAsCheckedIn()
    {
        WorkOrder workOrder = CheckIn(Intake);

        workOrder.ShouldSatisfyAllConditions(
            () => workOrder.Status.ShouldBe(WorkOrderStatus.CheckedIn),
            () => workOrder.CustomerId.ShouldBe(Intake.CustomerId),
            () => workOrder.BikeMakeModel.ShouldBe(Intake.BikeMakeModel),
            () => workOrder.BikeColour.ShouldBe(Intake.BikeColour),
            () => workOrder.JobType.ShouldBe(Intake.JobType),
            () => workOrder.WorkRequested.ShouldBe(Intake.WorkRequested),
            () => workOrder.EstimatedLabourMinutes.ShouldBe(Intake.EstimatedLabourMinutes),
            () => workOrder.LabourRateCentsPerHour.ShouldBe(Intake.LabourRateCentsPerHour),
            () => workOrder.EstimatedPartsCents.ShouldBe(Intake.EstimatedPartsCents),
            () => workOrder.PromisedOn.ShouldBe(Intake.PromisedOn),
            () => workOrder.AssignedToUserId.ShouldBe(Intake.AssignedToUserId),
            () => workOrder.CheckedInByUserId.ShouldBe(CounterStaffUserId),
            () => workOrder.CheckedInAtUtc.ShouldBe(CheckedInAtUtc),
            () => workOrder.StatusChangedAtUtc.ShouldBe(CheckedInAtUtc));
    }

    [Fact]
    public void TheEstimateIsLabourAtTheJobsRatePlusPartsRoundedToTheCent()
    {
        // 45 minutes at $85.50 an hour is $64.125, so this also checks that half a cent rounds up.
        WorkOrder workOrder = CheckIn(Intake with
        {
            EstimatedLabourMinutes = 45,
            LabourRateCentsPerHour = 8_550,
            EstimatedPartsCents = 1_200,
        });

        workOrder.EstimateTotalCents.ShouldBe(7_613);
    }
}

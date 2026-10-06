using BikeShop.Api.Domain.WorkOrders;

using Shouldly;

namespace BikeShop.Api.Tests.Domain;

public sealed class WorkOrderBillTests
{
    private const int CounterStaffUserId = 2;

    private const int MechanicUserId = 3;

    private static readonly DateTime CheckedInAtUtc = new(2026, 10, 5, 14, 30, 0, DateTimeKind.Utc);

    private static readonly DateTime LaterUtc = new(2026, 10, 6, 9, 0, 0, DateTimeKind.Utc);

    // 75 minutes at $95 an hour is $118.75 of labour, plus $42 of parts.
    private static readonly WorkOrderIntake TuneUpIntake = new(
        CustomerId: 7,
        BikeMakeModel: "Trek FX 2",
        BikeColour: "Matte black",
        JobType: JobType.TuneUp,
        WorkRequested: "Annual tune-up.",
        EstimatedLabourMinutes: 75,
        LabourRateCentsPerHour: 9_500,
        EstimatedPartsCents: 4_200,
        PromisedOn: new DateOnly(2026, 10, 9),
        AssignedToUserId: MechanicUserId);

    [Fact]
    public void TheBillIsTheEstimatedLabourPlusThePartsUsed()
    {
        WorkOrder workOrder = CheckIn(TuneUpIntake);

        workOrder.LogLabour(MechanicUserId, minutes: 120, note: "Seized derailleur bolt.", LaterUtc);
        workOrder.AddPart("Brake pads", quantity: 2, unitPriceCents: 1_500, LaterUtc);

        workOrder.ShouldSatisfyAllConditions(
            () => workOrder.BillCents.ShouldBe(14_875),
            () => workOrder.LoggedLabourMinutes.ShouldBe(120));
    }

    [Fact]
    public void PartsThatPushTheBillOverTheEstimateFlagTheJob()
    {
        WorkOrder workOrder = CheckIn(TuneUpIntake);

        workOrder.AddPart("Brake pads", quantity: 2, unitPriceCents: 1_500, LaterUtc);
        workOrder.IsOverEstimate.ShouldBeFalse();

        workOrder.AddPart("Chain", quantity: 1, unitPriceCents: 5_000, LaterUtc);
        workOrder.IsOverEstimate.ShouldBeTrue();
    }

    [Fact]
    public void RevisingTheEstimateWhenTheCustomerAgreesPutsTheExtraWorkOnTheBill()
    {
        WorkOrder workOrder = CheckIn(TuneUpIntake);
        workOrder.AddPart("Chain", quantity: 1, unitPriceCents: 5_000, LaterUtc);

        // The customer agrees to the $50 chain and 30 more minutes of labour.
        workOrder.UpdateDetails(TuneUpIntake with { EstimatedLabourMinutes = 105, EstimatedPartsCents = 9_200 });

        workOrder.ShouldSatisfyAllConditions(
            () => workOrder.BillCents.ShouldBe(21_625),
            () => workOrder.IsOverEstimate.ShouldBeFalse());
    }

    private static WorkOrder CheckIn(WorkOrderIntake intake) => WorkOrder.CheckIn(intake, CounterStaffUserId, CheckedInAtUtc);
}

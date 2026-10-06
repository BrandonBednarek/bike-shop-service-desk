using BikeShop.Api.Domain.WorkOrders;

using Shouldly;

using static BikeShop.Api.Tests.Domain.TuneUpJob;

namespace BikeShop.Api.Tests.Domain;

public sealed class WorkOrderBillTests
{
    [Fact]
    public void TheBillIsTheEstimatedLabourPlusThePartsUsed()
    {
        WorkOrder workOrder = CheckIn(Intake);

        workOrder.LogLabour(MechanicUserId, minutes: 120, note: "Seized derailleur bolt.", LaterUtc);
        workOrder.AddPart("Brake pads", quantity: 2, unitPriceCents: 1_500, LaterUtc);

        workOrder.ShouldSatisfyAllConditions(
            () => workOrder.BillCents.ShouldBe(14_875),
            () => workOrder.LoggedLabourMinutes.ShouldBe(120));
    }

    [Fact]
    public void PartsThatPushTheBillOverTheEstimateFlagTheJob()
    {
        WorkOrder workOrder = CheckIn(Intake);

        workOrder.AddPart("Brake pads", quantity: 2, unitPriceCents: 1_500, LaterUtc);
        workOrder.IsOverEstimate.ShouldBeFalse();

        workOrder.AddPart("Chain", quantity: 1, unitPriceCents: 5_000, LaterUtc);
        workOrder.IsOverEstimate.ShouldBeTrue();
    }

    [Fact]
    public void RevisingTheEstimateWhenTheCustomerAgreesPutsTheExtraWorkOnTheBill()
    {
        WorkOrder workOrder = CheckIn(Intake);
        workOrder.AddPart("Chain", quantity: 1, unitPriceCents: 5_000, LaterUtc);

        // The customer agrees to the $50 chain and 30 more minutes of labour.
        workOrder.UpdateDetails(Intake with { EstimatedLabourMinutes = 105, EstimatedPartsCents = 9_200 });

        workOrder.ShouldSatisfyAllConditions(
            () => workOrder.BillCents.ShouldBe(21_625),
            () => workOrder.IsOverEstimate.ShouldBeFalse());
    }
}

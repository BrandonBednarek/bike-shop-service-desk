using BikeShop.Api.Domain.WorkOrders;

namespace BikeShop.Api.Tests.Domain;

internal static class TuneUpJob
{
    public const int CounterStaffUserId = 2;

    public const int MechanicUserId = 3;

    public static readonly DateTime CheckedInAtUtc = new(2026, 10, 5, 14, 30, 0, DateTimeKind.Utc);

    public static readonly DateTime LaterUtc = new(2026, 10, 6, 9, 0, 0, DateTimeKind.Utc);

    // 75 minutes at $95 an hour is $118.75 of labour, plus $42 of parts: a $160.75 estimate.
    public static readonly WorkOrderIntake Intake = new(
        CustomerId: 7,
        BikeMakeModel: "Trek FX 2",
        BikeColour: "Matte black",
        JobType: JobType.TuneUp,
        WorkRequested: "Gears skip on the big cog and the brakes squeal.",
        EstimatedLabourMinutes: 75,
        LabourRateCentsPerHour: 9_500,
        EstimatedPartsCents: 4_200,
        PromisedOn: new DateOnly(2026, 10, 9),
        AssignedToUserId: MechanicUserId);

    public static WorkOrder CheckIn(WorkOrderIntake intake) => WorkOrder.CheckIn(intake, CounterStaffUserId, CheckedInAtUtc);
}

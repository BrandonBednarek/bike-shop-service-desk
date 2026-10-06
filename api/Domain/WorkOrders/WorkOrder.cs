namespace BikeShop.Api.Domain.WorkOrders;

public sealed class WorkOrder
{
    private WorkOrder()
    {
    }

    public int Id { get; private set; }

    public int CustomerId { get; private set; }

    public string BikeMakeModel { get; private set; } = string.Empty;

    public string BikeColour { get; private set; } = string.Empty;

    public JobType JobType { get; private set; }

    public string WorkRequested { get; private set; } = string.Empty;

    public int EstimatedLabourMinutes { get; private set; }

    public long LabourRateCentsPerHour { get; private set; }

    public long EstimatedPartsCents { get; private set; }

    public long EstimateTotalCents => LabourChargeCents(EstimatedLabourMinutes) + EstimatedPartsCents;

    public DateOnly PromisedOn { get; private set; }

    public int? AssignedToUserId { get; private set; }

    public WorkOrderStatus Status { get; private set; }

    public int CheckedInByUserId { get; private set; }

    public DateTime CheckedInAtUtc { get; private set; }

    public DateTime StatusChangedAtUtc { get; private set; }

    public static WorkOrder CheckIn(WorkOrderIntake intake, int checkedInByUserId, DateTime utcNow) => new()
    {
        CustomerId = intake.CustomerId,
        BikeMakeModel = intake.BikeMakeModel,
        BikeColour = intake.BikeColour,
        JobType = intake.JobType,
        WorkRequested = intake.WorkRequested,
        EstimatedLabourMinutes = intake.EstimatedLabourMinutes,
        LabourRateCentsPerHour = intake.LabourRateCentsPerHour,
        EstimatedPartsCents = intake.EstimatedPartsCents,
        PromisedOn = intake.PromisedOn,
        AssignedToUserId = intake.AssignedToUserId,
        Status = WorkOrderStatus.CheckedIn,
        CheckedInByUserId = checkedInByUserId,
        CheckedInAtUtc = utcNow,
        StatusChangedAtUtc = utcNow,
    };

    private long LabourChargeCents(int minutes) =>
        (long)Math.Round(minutes * LabourRateCentsPerHour / 60m, MidpointRounding.AwayFromZero);
}

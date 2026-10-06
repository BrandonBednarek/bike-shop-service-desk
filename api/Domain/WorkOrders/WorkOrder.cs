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

    public HoldReason? HoldReason { get; private set; }

    public string? PosReceiptNumber { get; private set; }

    public string? CancellationReason { get; private set; }

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

    public void Start(int startedByUserId, DateTime utcNow)
    {
        ChangeStatus(WorkOrderStatus.InProgress, utcNow);
        AssignedToUserId ??= startedByUserId;
    }

    public void Hold(HoldReason reason, DateTime utcNow)
    {
        ChangeStatus(WorkOrderStatus.OnHold, utcNow);
        HoldReason = reason;
    }

    public void MarkReady(DateTime utcNow) => ChangeStatus(WorkOrderStatus.ReadyForPickup, utcNow);

    public void Collect(string posReceiptNumber, DateTime utcNow)
    {
        ChangeStatus(WorkOrderStatus.Collected, utcNow);
        PosReceiptNumber = posReceiptNumber;
    }

    public void Cancel(string reason, DateTime utcNow)
    {
        ChangeStatus(WorkOrderStatus.Cancelled, utcNow);
        CancellationReason = reason;
    }

    public void Reopen(DateTime utcNow) => ChangeStatus(StatusAfterReopening(), utcNow);

    private void ChangeStatus(WorkOrderStatus next, DateTime utcNow)
    {
        if (!Status.CanChangeTo(next))
            throw new BusinessRuleException($"A job that is {Status} can't be moved to {next}.");

        Status = next;
        StatusChangedAtUtc = utcNow;
        ClearStatusDetails();
    }

    private void ClearStatusDetails()
    {
        HoldReason = null;
        PosReceiptNumber = null;
        CancellationReason = null;
    }

    private WorkOrderStatus StatusAfterReopening() =>
        Status == WorkOrderStatus.Collected ? WorkOrderStatus.ReadyForPickup : WorkOrderStatus.CheckedIn;

    private long LabourChargeCents(int minutes) =>
        (long)Math.Round(minutes * LabourRateCentsPerHour / 60m, MidpointRounding.AwayFromZero);
}

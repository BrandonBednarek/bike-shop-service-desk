namespace BikeShop.Api.Domain.WorkOrders;

public sealed class WorkOrder
{
    private readonly List<LabourEntry> _labourEntries = [];

    private readonly List<PartLine> _partLines = [];

    private readonly List<JobNote> _notes = [];

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

    public long EstimatedLabourCents => LabourChargeCents(EstimatedLabourMinutes);

    public long EstimateTotalCents => EstimatedLabourCents + EstimatedPartsCents;

    public DateOnly PromisedOn { get; private set; }

    public int? AssignedToUserId { get; private set; }

    public WorkOrderStatus Status { get; private set; }

    public bool IsClosed => Status is WorkOrderStatus.Collected or WorkOrderStatus.Cancelled;

    public int CheckedInByUserId { get; private set; }

    public DateTime CheckedInAtUtc { get; private set; }

    public DateTime StatusChangedAtUtc { get; private set; }

    public HoldReason? HoldReason { get; private set; }

    public string? PosReceiptNumber { get; private set; }

    public string? CancellationReason { get; private set; }

    public IReadOnlyList<LabourEntry> LabourEntries => _labourEntries;

    public IReadOnlyList<PartLine> PartLines => _partLines;

    public IReadOnlyList<JobNote> Notes => _notes;

    public int LoggedLabourMinutes => _labourEntries.Sum(entry => entry.Minutes);

    public long PartsCents => _partLines.Sum(part => part.TotalCents);

    public long BillCents => EstimatedLabourCents + PartsCents;

    public bool IsOverEstimate => BillCents > EstimateTotalCents;

    public static WorkOrder CheckIn(WorkOrderIntake intake, int checkedInByUserId, DateTime utcNow)
    {
        WorkOrder workOrder = new()
        {
            Status = WorkOrderStatus.CheckedIn,
            CheckedInByUserId = checkedInByUserId,
            CheckedInAtUtc = utcNow,
            StatusChangedAtUtc = utcNow,
        };
        workOrder.UpdateDetails(intake);
        return workOrder;
    }

    // Used at check-in and later, for example to revise the estimate once the customer agrees to more work.
    public void UpdateDetails(WorkOrderIntake intake)
    {
        CustomerId = intake.CustomerId;
        BikeMakeModel = intake.BikeMakeModel;
        BikeColour = intake.BikeColour;
        JobType = intake.JobType;
        WorkRequested = intake.WorkRequested;
        EstimatedLabourMinutes = intake.EstimatedLabourMinutes;
        LabourRateCentsPerHour = intake.LabourRateCentsPerHour;
        EstimatedPartsCents = intake.EstimatedPartsCents;
        PromisedOn = intake.PromisedOn;
        AssignedToUserId = intake.AssignedToUserId;
    }

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

    public void LogLabour(int mechanicUserId, int minutes, string? note, DateTime utcNow) =>
        _labourEntries.Add(LabourEntry.Create(mechanicUserId, minutes, note, utcNow));

    public void AddPart(string description, int quantity, long unitPriceCents, DateTime utcNow) =>
        _partLines.Add(PartLine.Create(description, quantity, unitPriceCents, utcNow));

    public void AddNote(string text, int writtenByUserId, DateTime utcNow) =>
        _notes.Add(JobNote.Create(text, writtenByUserId, utcNow));

    public void RemoveLabour(int labourEntryId)
    {
        if (_labourEntries.RemoveAll(entry => entry.Id == labourEntryId) == 0)
            throw new BusinessRuleException("This job has no labour entry with that ID.");
    }

    public void RemovePart(int partLineId)
    {
        if (_partLines.RemoveAll(part => part.Id == partLineId) == 0)
            throw new BusinessRuleException("This job has no part with that ID.");
    }

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

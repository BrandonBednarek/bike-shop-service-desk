using BikeShop.Api.Domain.Customers;
using BikeShop.Api.Domain.WorkOrders;

namespace BikeShop.Api.Features.WorkOrders;

public sealed record WorkOrderResponse(
    int Id,
    int CustomerId,
    string CustomerName,
    string CustomerPhone,
    string BikeMakeModel,
    string BikeColour,
    JobType JobType,
    string WorkRequested,
    int EstimatedLabourMinutes,
    long LabourRateCentsPerHour,
    long EstimatedPartsCents,
    long EstimateTotalCents,
    long BillCents,
    bool IsOverEstimate,
    int LoggedLabourMinutes,
    DateOnly PromisedOn,
    int? AssignedToUserId,
    WorkOrderStatus Status,
    DateTime StatusChangedAtUtc,
    HoldReason? HoldReason,
    string? PosReceiptNumber,
    string? CancellationReason,
    int CheckedInByUserId,
    DateTime CheckedInAtUtc,
    IReadOnlyList<LabourEntryResponse> LabourEntries,
    IReadOnlyList<PartLineResponse> PartLines,
    IReadOnlyList<JobNoteResponse> Notes)
{
    public static WorkOrderResponse From(WorkOrder workOrder, Customer customer) => new(
        workOrder.Id,
        workOrder.CustomerId,
        customer.Name,
        customer.Phone,
        workOrder.BikeMakeModel,
        workOrder.BikeColour,
        workOrder.JobType,
        workOrder.WorkRequested,
        workOrder.EstimatedLabourMinutes,
        workOrder.LabourRateCentsPerHour,
        workOrder.EstimatedPartsCents,
        workOrder.EstimateTotalCents,
        workOrder.BillCents,
        workOrder.IsOverEstimate,
        workOrder.LoggedLabourMinutes,
        workOrder.PromisedOn,
        workOrder.AssignedToUserId,
        workOrder.Status,
        workOrder.StatusChangedAtUtc,
        workOrder.HoldReason,
        workOrder.PosReceiptNumber,
        workOrder.CancellationReason,
        workOrder.CheckedInByUserId,
        workOrder.CheckedInAtUtc,
        [.. workOrder.LabourEntries.Select(LabourEntryResponse.From)],
        [.. workOrder.PartLines.Select(PartLineResponse.From)],
        [.. workOrder.Notes.Select(JobNoteResponse.From)]);
}

public sealed record LabourEntryResponse(int Id, int MechanicUserId, int Minutes, string? Note, DateTime LoggedAtUtc)
{
    public static LabourEntryResponse From(LabourEntry entry) =>
        new(entry.Id, entry.MechanicUserId, entry.Minutes, entry.Note, entry.LoggedAtUtc);
}

public sealed record PartLineResponse(int Id, string Description, int Quantity, long UnitPriceCents, long TotalCents, DateTime AddedAtUtc)
{
    public static PartLineResponse From(PartLine part) =>
        new(part.Id, part.Description, part.Quantity, part.UnitPriceCents, part.TotalCents, part.AddedAtUtc);
}

public sealed record JobNoteResponse(int Id, string Text, int WrittenByUserId, DateTime WrittenAtUtc)
{
    public static JobNoteResponse From(JobNote note) => new(note.Id, note.Text, note.WrittenByUserId, note.WrittenAtUtc);
}

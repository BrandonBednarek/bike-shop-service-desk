namespace BikeShop.Api.Domain.WorkOrders;

public sealed class LabourEntry
{
    private LabourEntry()
    {
    }

    public int Id { get; private set; }

    public int MechanicUserId { get; private set; }

    public int Minutes { get; private set; }

    public string? Note { get; private set; }

    public DateTime LoggedAtUtc { get; private set; }

    internal static LabourEntry Create(int mechanicUserId, int minutes, string? note, DateTime utcNow) => new()
    {
        MechanicUserId = mechanicUserId,
        Minutes = minutes,
        Note = note,
        LoggedAtUtc = utcNow,
    };
}

namespace BikeShop.Api.Domain.WorkOrders;

public sealed class JobNote
{
    private JobNote()
    {
    }

    public int Id { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public int WrittenByUserId { get; private set; }

    public DateTime WrittenAtUtc { get; private set; }

    internal static JobNote Create(string text, int writtenByUserId, DateTime utcNow) => new()
    {
        Text = text,
        WrittenByUserId = writtenByUserId,
        WrittenAtUtc = utcNow,
    };
}

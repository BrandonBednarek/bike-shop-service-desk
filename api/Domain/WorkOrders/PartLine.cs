namespace BikeShop.Api.Domain.WorkOrders;

public sealed class PartLine
{
    private PartLine()
    {
    }

    public int Id { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public long UnitPriceCents { get; private set; }

    public long TotalCents => Quantity * UnitPriceCents;

    public DateTime AddedAtUtc { get; private set; }

    internal static PartLine Create(string description, int quantity, long unitPriceCents, DateTime utcNow) => new()
    {
        Description = description,
        Quantity = quantity,
        UnitPriceCents = unitPriceCents,
        AddedAtUtc = utcNow,
    };
}

namespace BikeShop.Api.Infrastructure.Seeding;

/// <summary>
/// The <c>Demo</c> settings. Demo mode fills an empty shop with sample jobs and lists the demo
/// accounts on the sign-in page. It's off unless turned on, as Docker Compose and the local
/// launch profile do with <c>Demo__Enabled=true</c>.
/// </summary>
public sealed class DemoOptions
{
    public const string SectionName = "Demo";

    public bool Enabled { get; set; }
}

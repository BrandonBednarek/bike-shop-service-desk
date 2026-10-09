using BikeShop.Api.Domain.Customers;
using BikeShop.Api.Domain.WorkOrders;
using BikeShop.Api.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BikeShop.Api.Infrastructure.Seeding;

/// <summary>
/// Fills an empty shop with sample jobs, one for each situation the app is built to show. Each
/// job is replayed through the same domain methods the API uses, with times in the past, so the
/// demo only holds jobs the app itself could have made. The jobs are in DemoJobSeeder.Scenarios.cs.
/// </summary>
public sealed partial class DemoJobSeeder(AppDbContext dbContext, TimeProvider clock, ILogger<DemoJobSeeder> logger)
{
    private const long ShopRateCentsPerHour = 9_500;

    private readonly DateTime _utcNow = clock.GetUtcNow().UtcDateTime;

    // The browser works out overdue jobs from its own date, which is at most a day either side of
    // the UTC date. So jobs meant to look overdue were promised at least two days ago, and other
    // open jobs at least a day ahead. Ready and closed jobs never look overdue, whatever the date.
    private DateOnly Today => DateOnly.FromDateTime(_utcNow);

    public async Task SeedIfEmptyAsync()
    {
        if (await dbContext.Customers.AnyAsync())
            return;

        DemoUserIds userIds = await LoadDemoUserIdsAsync();

        // All or nothing, so a start-up stopped partway can't leave a half-filled shop that the
        // check above would then never fill.
        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync();

        // Oldest check-in first, so job numbers go up with check-in time, as they would for real.
        await SeedOverhaulReadyForOverAWeekAsync(userIds);
        await SeedTuneUpCollectedYesterdayAsync(userIds);
        await SeedForkServiceCancelledOnBackorderAsync(userIds);
        await SeedDerailleurStalledWaitingForPartsAsync(userIds);
        await SeedTuneUpOverdueAndNeverStartedAsync(userIds);
        await SeedEBikeOverdueInProgressAsync(userIds);
        await SeedBrakesWaitingForCustomerApprovalAsync(userIds);
        await SeedFlatRepairReadyForPickupAsync(userIds);
        await SeedWheelSlightlyOverEstimateAsync(userIds);
        await SeedBuildCheckedInTodayAsync(userIds);
        await transaction.CommitAsync();

        logger.LogInformation("Seeded {Count} demo jobs", await dbContext.WorkOrders.CountAsync());
    }

    private async Task<DemoUserIds> LoadDemoUserIdsAsync()
    {
        Dictionary<string, int> userIdsByUsername = await dbContext.Users.ToDictionaryAsync(user => user.Username, user => user.Id);
        return new DemoUserIds(
            Owner: userIdsByUsername["owner"],
            Lebis: userIdsByUsername["lebis"],
            Harry: userIdsByUsername["harry"],
            Mack: userIdsByUsername["mack"]);
    }

    // Saved straight away, because a job needs its customer's ID.
    private async Task<Customer> AddCustomerAsync(string name, string phone, string? email)
    {
        Customer customer = Customer.Create(name, phone, email);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();
        return customer;
    }

    // One job per save, so each one takes the next job number in turn.
    private async Task AddWorkOrderAsync(WorkOrder workOrder)
    {
        dbContext.WorkOrders.Add(workOrder);
        await dbContext.SaveChangesAsync();
    }

    private DateTime DaysAgo(int days) => _utcNow.AddDays(-days);

    private DateTime HoursAgo(int hours) => _utcNow.AddHours(-hours);

    private sealed record DemoUserIds(int Owner, int Lebis, int Harry, int Mack);
}

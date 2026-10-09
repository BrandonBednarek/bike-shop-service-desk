using BikeShop.Api.Domain.Customers;
using BikeShop.Api.Domain.WorkOrders;

namespace BikeShop.Api.Infrastructure.Seeding;

// The sample jobs, one method each, named after what the job shows. Job numbers start at #1001
// and follow the order DemoJobSeeder runs these in.
public sealed partial class DemoJobSeeder
{
    // #1001: finished over a week ago and still not collected. The labour ran 45 minutes over
    // the estimate, which the shop absorbs.
    private async Task SeedOverhaulReadyForOverAWeekAsync(DemoUserIds userIds)
    {
        Customer customer = await AddCustomerAsync("Bilbo Baggins", "(416) 555-0107", "bilbo.baggins@example.com");
        WorkOrder workOrder = WorkOrder.CheckIn(
            new WorkOrderIntake(
                customer.Id,
                BikeMakeModel: "Rocky Mountain Element 30",
                BikeColour: "Red",
                JobType: JobType.Overhaul,
                WorkRequested: "Full overhaul before I sell it. The shifting, the brakes, everything feels worn out.",
                EstimatedLabourMinutes: 180,
                LabourRateCentsPerHour: ShopRateCentsPerHour,
                EstimatedPartsCents: 12_000,
                PromisedOn: Today.AddDays(-7),
                AssignedToUserId: userIds.Lebis),
            userIds.Lebis,
            DaysAgo(10));

        workOrder.Start(userIds.Lebis, DaysAgo(9));
        workOrder.AddPart("Chain, 11-speed", quantity: 1, unitPriceCents: 5_000, DaysAgo(9).AddHours(1));
        workOrder.AddPart("Cassette, 11-speed", quantity: 1, unitPriceCents: 6_500, DaysAgo(9).AddHours(1));
        workOrder.LogLabour(userIds.Lebis, minutes: 120, "Stripped and cleaned everything, and regreased the bearings.", DaysAgo(9).AddHours(4));
        workOrder.LogLabour(userIds.Lebis, minutes: 105, "Fitted the new chain and cassette, bled both brakes and test rode it.", DaysAgo(8).AddHours(3));
        workOrder.MarkReady(DaysAgo(8).AddHours(3));
        workOrder.AddNote("Called to say it's ready. Left a voicemail.", userIds.Owner, DaysAgo(6));
        workOrder.AddNote("Called again, no answer. Sent an email as well.", userIds.Harry, DaysAgo(2));

        await AddWorkOrderAsync(workOrder);
    }

    // #1002: a closed job, so it's off the board; open it at /work-orders/1002. Mack did the
    // work before that account was deactivated, and the name still shows on the labour.
    private async Task SeedTuneUpCollectedYesterdayAsync(DemoUserIds userIds)
    {
        Customer customer = await AddCustomerAsync("Samwise Gamgee", "(416) 555-0118", null);
        WorkOrder workOrder = WorkOrder.CheckIn(
            new WorkOrderIntake(
                customer.Id,
                BikeMakeModel: "Trek Domane AL 2",
                BikeColour: "Blue",
                JobType: JobType.TuneUp,
                WorkRequested: "Getting it ready for a long charity ride. The gears are noisy and the brakes squeal.",
                EstimatedLabourMinutes: 75,
                LabourRateCentsPerHour: ShopRateCentsPerHour,
                EstimatedPartsCents: 3_000,
                PromisedOn: Today.AddDays(-5),
                AssignedToUserId: userIds.Mack),
            userIds.Harry,
            DaysAgo(8));

        workOrder.Start(userIds.Mack, DaysAgo(7));
        workOrder.AddPart("Rim brake pads, pair", quantity: 2, unitPriceCents: 1_500, DaysAgo(7).AddHours(1));
        workOrder.LogLabour(userIds.Mack, minutes: 70, "Tuned the gears, trued both wheels and fitted new pads front and rear.", DaysAgo(7).AddHours(2));
        workOrder.MarkReady(DaysAgo(7).AddHours(2));
        workOrder.Collect("104187", DaysAgo(1));

        await AddWorkOrderAsync(workOrder);
    }

    // #1003: cancelled because the customer wanted to ride the bike while a part was on
    // backorder. Closed, so open it at /work-orders/1003.
    private async Task SeedForkServiceCancelledOnBackorderAsync(DemoUserIds userIds)
    {
        Customer customer = await AddCustomerAsync("Alfred Borden", "(647) 555-0124", "alfred.borden@example.com");
        WorkOrder workOrder = WorkOrder.CheckIn(
            new WorkOrderIntake(
                customer.Id,
                BikeMakeModel: "Rocky Mountain Growler 40",
                BikeColour: "Orange",
                JobType: JobType.Suspension,
                WorkRequested: "The fork feels sticky and there's oil on it after every ride.",
                EstimatedLabourMinutes: 90,
                LabourRateCentsPerHour: ShopRateCentsPerHour,
                EstimatedPartsCents: 6_500,
                PromisedOn: Today.AddDays(-3),
                AssignedToUserId: userIds.Harry),
            userIds.Owner,
            DaysAgo(7));

        workOrder.Start(userIds.Harry, DaysAgo(6));
        workOrder.Hold(HoldReason.WaitingForParts, DaysAgo(6).AddHours(2));
        workOrder.AddNote("Both fork seals are torn. Ordered a seal kit; the supplier says 2 to 3 weeks.", userIds.Harry, DaysAgo(6).AddHours(2));
        workOrder.Cancel("The customer took the bike home to ride while the seal kit is on backorder, and will book it in again when the kit arrives.", DaysAgo(5));

        await AddWorkOrderAsync(workOrder);
    }

    // #1004: overdue and on hold for parts, on the dashboard under On hold.
    private async Task SeedDerailleurStalledWaitingForPartsAsync(DemoUserIds userIds)
    {
        Customer customer = await AddCustomerAsync("Marty McFly", "(416) 555-0131", null);
        WorkOrder workOrder = WorkOrder.CheckIn(
            new WorkOrderIntake(
                customer.Id,
                BikeMakeModel: "Norco Search XR",
                BikeColour: "Blue",
                JobType: JobType.Drivetrain,
                WorkRequested: "Crashed on a gravel ride and bent the rear derailleur. Now it won't shift onto the big cogs.",
                EstimatedLabourMinutes: 60,
                LabourRateCentsPerHour: ShopRateCentsPerHour,
                EstimatedPartsCents: 6_000,
                PromisedOn: Today.AddDays(-3),
                AssignedToUserId: userIds.Harry),
            userIds.Lebis,
            DaysAgo(6));

        workOrder.Start(userIds.Harry, DaysAgo(5));
        workOrder.LogLabour(userIds.Harry, minutes: 30, "Straightened the derailleur cage. The hanger is bent too far to save.", DaysAgo(5).AddHours(1));
        workOrder.Hold(HoldReason.WaitingForParts, DaysAgo(5).AddHours(1));
        workOrder.AddNote("Ordered a new derailleur hanger. The supplier says it'll arrive next week.", userIds.Harry, DaysAgo(5).AddHours(1));
        workOrder.AddNote("The customer called to check on it. Told them we're still waiting for the hanger.", userIds.Lebis, DaysAgo(1));

        await AddWorkOrderAsync(workOrder);
    }

    // #1005: overdue, never started and not assigned to anyone, a sign of too much work booked in.
    private async Task SeedTuneUpOverdueAndNeverStartedAsync(DemoUserIds userIds)
    {
        Customer customer = await AddCustomerAsync("Truman Burbank", "(905) 555-0142", "truman.burbank@example.com");
        WorkOrder workOrder = WorkOrder.CheckIn(
            new WorkOrderIntake(
                customer.Id,
                BikeMakeModel: "Trek FX 3",
                BikeColour: "Grey",
                JobType: JobType.TuneUp,
                WorkRequested: "I ride it to work every day. The gears slip on hills and the back brake feels soft.",
                EstimatedLabourMinutes: 75,
                LabourRateCentsPerHour: ShopRateCentsPerHour,
                EstimatedPartsCents: 3_000,
                PromisedOn: Today.AddDays(-2),
                AssignedToUserId: null),
            userIds.Harry,
            DaysAgo(5));

        await AddWorkOrderAsync(workOrder);
    }

    // #1006: overdue while the mechanic is still working on it.
    private async Task SeedEBikeOverdueInProgressAsync(DemoUserIds userIds)
    {
        Customer customer = await AddCustomerAsync("Emmett Brown", "(647) 555-0153", null);
        WorkOrder workOrder = WorkOrder.CheckIn(
            new WorkOrderIntake(
                customer.Id,
                BikeMakeModel: "Giant Explore E+ 1",
                BikeColour: "Silver",
                JobType: JobType.EBike,
                WorkRequested: "The motor keeps cutting out over bumps and the display flashes a speed sensor error.",
                EstimatedLabourMinutes: 90,
                LabourRateCentsPerHour: ShopRateCentsPerHour,
                EstimatedPartsCents: 0,
                PromisedOn: Today.AddDays(-2),
                AssignedToUserId: userIds.Lebis),
            userIds.Harry,
            DaysAgo(4));

        workOrder.Start(userIds.Lebis, DaysAgo(3));
        workOrder.LogLabour(userIds.Lebis, minutes: 60, "Cleaned and reseated the speed sensor and its magnet. The error came back on a test ride.", DaysAgo(3).AddHours(2));
        workOrder.AddNote("Borrowing the dealer's diagnostic tool to read the motor's error log.", userIds.Lebis, DaysAgo(1));

        await AddWorkOrderAsync(workOrder);
    }

    // #1007: a new rotor would take the bill over the estimate, so the job is on hold until the
    // customer agrees. The note is the one the app writes when staff choose to hold the job.
    private async Task SeedBrakesWaitingForCustomerApprovalAsync(DemoUserIds userIds)
    {
        Customer customer = await AddCustomerAsync("Henry Fonda", "(416) 555-0166", "henry.fonda@example.com");
        WorkOrder workOrder = WorkOrder.CheckIn(
            new WorkOrderIntake(
                customer.Id,
                BikeMakeModel: "Giant Escape 2 Disc",
                BikeColour: "Black",
                JobType: JobType.Brakes,
                WorkRequested: "The brakes squeal and I have to squeeze really hard to stop.",
                EstimatedLabourMinutes: 60,
                LabourRateCentsPerHour: ShopRateCentsPerHour,
                EstimatedPartsCents: 6_000,
                PromisedOn: Today.AddDays(2),
                AssignedToUserId: userIds.Lebis),
            userIds.Owner,
            DaysAgo(3));

        workOrder.Start(userIds.Lebis, DaysAgo(2));
        workOrder.AddPart("Disc brake pads, pair", quantity: 2, unitPriceCents: 3_000, DaysAgo(2).AddHours(1));
        workOrder.LogLabour(userIds.Lebis, minutes: 45, "Fitted new pads front and rear. The front rotor is worn below its minimum thickness.", DaysAgo(2).AddHours(1));
        workOrder.Hold(HoldReason.WaitingForCustomer, DaysAgo(2).AddHours(1));
        workOrder.AddNote("Ask the customer about 1 × Front rotor, 160 mm ($45.00). It would take the bill $45.00 over the estimate.", userIds.Lebis, DaysAgo(2).AddHours(1));
        workOrder.AddNote("Called and left a voicemail about the rotor.", userIds.Lebis, DaysAgo(2).AddHours(2));

        await AddWorkOrderAsync(workOrder);
    }

    // #1008: ready for two days, the second bike on the dashboard's ready table.
    private async Task SeedFlatRepairReadyForPickupAsync(DemoUserIds userIds)
    {
        Customer customer = await AddCustomerAsync("Guido Orefice", "(905) 555-0172", null);
        WorkOrder workOrder = WorkOrder.CheckIn(
            new WorkOrderIntake(
                customer.Id,
                BikeMakeModel: "Specialized Jett 20",
                BikeColour: "Green",
                JobType: JobType.FlatRepair,
                WorkRequested: "My son's back tyre keeps going flat overnight.",
                EstimatedLabourMinutes: 15,
                LabourRateCentsPerHour: ShopRateCentsPerHour,
                EstimatedPartsCents: 1_200,
                PromisedOn: Today.AddDays(1),
                AssignedToUserId: userIds.Harry),
            userIds.Harry,
            DaysAgo(3).AddHours(2));

        workOrder.Start(userIds.Harry, DaysAgo(2));
        workOrder.AddPart("Inner tube, 20 inch", quantity: 1, unitPriceCents: 1_200, DaysAgo(2).AddHours(1));
        workOrder.LogLabour(userIds.Harry, minutes: 15, "Pulled a thorn out of the tyre and fitted a new tube.", DaysAgo(2).AddHours(1));
        workOrder.MarkReady(DaysAgo(2).AddHours(1));

        await AddWorkOrderAsync(workOrder);
    }

    // #1009: $4 over the estimate for rim tape, too little to call the customer about, so the
    // mechanic added it and carried on.
    private async Task SeedWheelSlightlyOverEstimateAsync(DemoUserIds userIds)
    {
        Customer customer = await AddCustomerAsync("Will Hunting", "(416) 555-0185", "will.hunting@example.com");
        WorkOrder workOrder = WorkOrder.CheckIn(
            new WorkOrderIntake(
                customer.Id,
                BikeMakeModel: "Specialized Sirrus 2.0",
                BikeColour: "White",
                JobType: JobType.Wheel,
                WorkRequested: "The back wheel wobbles and rubs on the brake. I think a spoke snapped.",
                EstimatedLabourMinutes: 45,
                LabourRateCentsPerHour: ShopRateCentsPerHour,
                EstimatedPartsCents: 1_000,
                PromisedOn: Today.AddDays(2),
                AssignedToUserId: userIds.Harry),
            userIds.Lebis,
            DaysAgo(2));

        workOrder.Start(userIds.Harry, HoursAgo(3));
        workOrder.AddPart("Spoke", quantity: 2, unitPriceCents: 300, HoursAgo(2));
        workOrder.AddPart("Rim tape", quantity: 1, unitPriceCents: 800, HoursAgo(2));
        workOrder.LogLabour(userIds.Harry, minutes: 40, "Replaced two broken spokes and trued the wheel.", HoursAgo(1));
        workOrder.AddNote("The rim tape was torn too, so I replaced it. It's $4 over the estimate, too little to call about.", userIds.Harry, HoursAgo(1));

        await AddWorkOrderAsync(workOrder);
    }

    // #1010: just checked in, nothing done yet.
    private async Task SeedBuildCheckedInTodayAsync(DemoUserIds userIds)
    {
        Customer customer = await AddCustomerAsync("Robert Angier", "(647) 555-0198", null);
        WorkOrder workOrder = WorkOrder.CheckIn(
            new WorkOrderIntake(
                customer.Id,
                BikeMakeModel: "Norco Indie 3",
                BikeColour: "Dark green",
                JobType: JobType.Assembly,
                WorkRequested: "I bought this online and it came in a box. Can you build it and check it's safe to ride?",
                EstimatedLabourMinutes: 90,
                LabourRateCentsPerHour: ShopRateCentsPerHour,
                EstimatedPartsCents: 0,
                PromisedOn: Today.AddDays(3),
                AssignedToUserId: userIds.Lebis),
            userIds.Owner,
            HoursAgo(2));

        await AddWorkOrderAsync(workOrder);
    }
}

using BikeShop.Api.Domain.WorkOrders;

using Shouldly;

namespace BikeShop.Api.Tests.Domain;

public sealed class WorkOrderStatusTransitionsTests
{
    private static readonly (WorkOrderStatus Current, WorkOrderStatus Next)[] AgreedChanges =
    [
        (WorkOrderStatus.CheckedIn, WorkOrderStatus.InProgress),
        (WorkOrderStatus.CheckedIn, WorkOrderStatus.OnHold),
        (WorkOrderStatus.CheckedIn, WorkOrderStatus.Cancelled),
        (WorkOrderStatus.InProgress, WorkOrderStatus.OnHold),
        (WorkOrderStatus.InProgress, WorkOrderStatus.ReadyForPickup),
        (WorkOrderStatus.InProgress, WorkOrderStatus.Cancelled),
        (WorkOrderStatus.OnHold, WorkOrderStatus.InProgress),
        (WorkOrderStatus.OnHold, WorkOrderStatus.ReadyForPickup),
        (WorkOrderStatus.OnHold, WorkOrderStatus.Cancelled),
        (WorkOrderStatus.ReadyForPickup, WorkOrderStatus.Collected),
        (WorkOrderStatus.ReadyForPickup, WorkOrderStatus.InProgress),
        (WorkOrderStatus.Collected, WorkOrderStatus.ReadyForPickup),
        (WorkOrderStatus.Cancelled, WorkOrderStatus.CheckedIn),
    ];

    [Fact]
    public void OnlyTheAgreedStatusChangesAreAllowed()
    {
        IEnumerable<(WorkOrderStatus Current, WorkOrderStatus Next)> allowedChanges =
            EveryPossibleChange().Where(change => change.Current.CanChangeTo(change.Next));

        allowedChanges.ShouldBe(AgreedChanges, ignoreOrder: true);
    }

    private static IEnumerable<(WorkOrderStatus Current, WorkOrderStatus Next)> EveryPossibleChange() =>
        Enum.GetValues<WorkOrderStatus>().SelectMany(current =>
            Enum.GetValues<WorkOrderStatus>().Select(next => (current, next)));
}

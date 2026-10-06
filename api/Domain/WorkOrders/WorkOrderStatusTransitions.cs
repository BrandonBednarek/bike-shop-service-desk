namespace BikeShop.Api.Domain.WorkOrders;

public static class WorkOrderStatusTransitions
{
    private static readonly Dictionary<WorkOrderStatus, WorkOrderStatus[]> AllowedNextStatuses = new()
    {
        [WorkOrderStatus.CheckedIn] = [WorkOrderStatus.InProgress, WorkOrderStatus.OnHold, WorkOrderStatus.Cancelled],
        [WorkOrderStatus.InProgress] = [WorkOrderStatus.OnHold, WorkOrderStatus.ReadyForPickup, WorkOrderStatus.Cancelled],
        [WorkOrderStatus.OnHold] = [WorkOrderStatus.InProgress, WorkOrderStatus.ReadyForPickup, WorkOrderStatus.Cancelled],
        [WorkOrderStatus.ReadyForPickup] = [WorkOrderStatus.Collected, WorkOrderStatus.InProgress],
        [WorkOrderStatus.Collected] = [WorkOrderStatus.ReadyForPickup],
        [WorkOrderStatus.Cancelled] = [WorkOrderStatus.CheckedIn],
    };

    public static bool CanChangeTo(this WorkOrderStatus current, WorkOrderStatus next) => AllowedNextStatuses[current].Contains(next);
}

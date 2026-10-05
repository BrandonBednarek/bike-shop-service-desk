namespace BikeShop.Api.Domain.WorkOrders;

public enum WorkOrderStatus
{
    CheckedIn,
    InProgress,
    OnHold,
    ReadyForPickup,
    Collected,
    Cancelled,
}

namespace BikeShop.Api.Domain.WorkOrders;

public sealed record WorkOrderIntake(
    int CustomerId,
    string BikeMakeModel,
    string BikeColour,
    JobType JobType,
    string WorkRequested,
    DateOnly PromisedOn,
    int? AssignedToUserId);

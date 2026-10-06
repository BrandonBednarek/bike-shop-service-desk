namespace BikeShop.Api.Domain.WorkOrders;

public sealed record WorkOrderIntake(
    int CustomerId,
    string BikeMakeModel,
    string BikeColour,
    JobType JobType,
    string WorkRequested,
    int EstimatedLabourMinutes,
    long LabourRateCentsPerHour,
    long EstimatedPartsCents,
    DateOnly PromisedOn,
    int? AssignedToUserId);

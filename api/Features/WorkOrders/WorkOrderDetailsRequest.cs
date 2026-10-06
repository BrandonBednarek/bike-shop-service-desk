using System.ComponentModel.DataAnnotations;

using BikeShop.Api.Domain.WorkOrders;

namespace BikeShop.Api.Features.WorkOrders;

// Numbers, the job type and the date are nullable so a missing value fails [Required]
// instead of quietly arriving as 0, TuneUp or 0001-01-01.
public sealed record WorkOrderDetailsRequest(
    [Required] int? CustomerId,
    [Required, MaxLength(100)] string BikeMakeModel,
    [Required, MaxLength(50)] string BikeColour,
    [Required] JobType? JobType,
    [Required, MaxLength(2000)] string WorkRequested,
    [Required, Range(0, int.MaxValue)] int? EstimatedLabourMinutes,
    [Required, Range(0, long.MaxValue)] long? LabourRateCentsPerHour,
    [Required, Range(0, long.MaxValue)] long? EstimatedPartsCents,
    [Required] DateOnly? PromisedOn,
    int? AssignedToUserId);

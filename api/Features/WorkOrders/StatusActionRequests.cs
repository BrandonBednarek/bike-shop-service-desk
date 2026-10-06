using System.ComponentModel.DataAnnotations;

using BikeShop.Api.Domain.WorkOrders;

namespace BikeShop.Api.Features.WorkOrders;

public sealed record HoldRequest([Required] HoldReason? Reason, [MaxLength(1000)] string? Note);

public sealed record CollectRequest([Required, MaxLength(50)] string PosReceiptNumber);

public sealed record CancelRequest([Required, MaxLength(500)] string Reason);

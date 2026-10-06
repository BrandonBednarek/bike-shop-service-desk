using System.ComponentModel.DataAnnotations;

namespace BikeShop.Api.Features.WorkOrders;

public sealed record LogLabourRequest(int? MechanicUserId, [Required, Range(1, 24 * 60)] int? Minutes, [MaxLength(500)] string? Note);

public sealed record AddPartRequest(
    [Required, MaxLength(200)] string Description,
    [Required, Range(1, 1000)] int? Quantity,
    [Required, Range(0, long.MaxValue)] long? UnitPriceCents);

public sealed record AddNoteRequest([Required, MaxLength(1000)] string Text);

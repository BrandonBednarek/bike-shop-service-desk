using System.ComponentModel.DataAnnotations;

namespace BikeShop.Api.Features.Customers;

public sealed record CreateCustomerRequest(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(30)] string Phone,
    [MaxLength(200), EmailAddress] string? Email);

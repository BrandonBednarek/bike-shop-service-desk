namespace BikeShop.Api.Features.Customers;

public sealed record CustomerResponse(int Id, string Name, string Phone, string? Email);

using BikeShop.Api.Domain.Customers;
using BikeShop.Api.Infrastructure.Persistence;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BikeShop.Api.Features.Customers;

public static class CustomerEndpoints
{
    private const int MaxSearchResults = 20;

    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder customers = endpoints.MapGroup("/api/customers").RequireAuthorization();

        customers.MapGet("/", SearchCustomersAsync);
        customers.MapPost("/", CreateCustomerAsync);

        return endpoints;
    }

    private static async Task<Ok<List<CustomerResponse>>> SearchCustomersAsync(
        string search,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        string namePattern = $"%{search}%";
        string phoneDigits = PhoneNumbers.ToDigits(search);

        List<CustomerResponse> matches = await dbContext.Customers
            .Where(customer => EF.Functions.Like(customer.Name, namePattern)
                || (phoneDigits != string.Empty && customer.PhoneDigits.Contains(phoneDigits)))
            .OrderBy(customer => customer.Name)
            .Take(MaxSearchResults)
            .Select(customer => ToResponse(customer))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(matches);
    }

    private static async Task<Ok<CustomerResponse>> CreateCustomerAsync(
        CreateCustomerRequest request,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Customer customer = Customer.Create(request.Name, request.Phone, request.Email);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(ToResponse(customer));
    }

    private static CustomerResponse ToResponse(Customer customer) =>
        new(customer.Id, customer.Name, customer.Phone, customer.Email);
}

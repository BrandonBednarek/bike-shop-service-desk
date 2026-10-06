using BikeShop.Api.Domain.Customers;
using BikeShop.Api.Domain.WorkOrders;
using BikeShop.Api.Infrastructure.Persistence;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BikeShop.Api.Features.WorkOrders;

internal static class WorkOrderResults
{
    public static ProblemHttpResult UnknownJob() => TypedResults.Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "There's no job with that number.");

    public static async Task<Ok<WorkOrderResponse>> OkAsync(WorkOrder workOrder, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        Customer customer = await dbContext.Customers.SingleAsync(candidate => candidate.Id == workOrder.CustomerId, cancellationToken);
        return TypedResults.Ok(WorkOrderResponse.From(workOrder, customer));
    }
}

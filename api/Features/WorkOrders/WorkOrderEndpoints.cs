using System.Security.Claims;

using BikeShop.Api.Domain.Customers;
using BikeShop.Api.Domain.WorkOrders;
using BikeShop.Api.Features.Auth;
using BikeShop.Api.Infrastructure.Persistence;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BikeShop.Api.Features.WorkOrders;

public static class WorkOrderEndpoints
{
    public static IEndpointRouteBuilder MapWorkOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder workOrders = endpoints.MapGroup("/api/work-orders").RequireAuthorization();

        workOrders.MapPost("/", CheckInAsync);
        workOrders.MapGet("/", ListAsync);
        workOrders.MapGet("/{id:int}", GetAsync);

        return endpoints;
    }

    private static async Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> CheckInAsync(
        WorkOrderDetailsRequest request,
        ClaimsPrincipal user,
        AppDbContext dbContext,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        Customer? customer = await dbContext.Customers.FindAsync([request.CustomerId], cancellationToken);
        if (customer is null)
            return UnknownCustomerProblem();

        WorkOrder workOrder = WorkOrder.CheckIn(ToIntake(request), SessionClaims.ReadCurrentUser(user).Id, clock.GetUtcNow().UtcDateTime);
        dbContext.WorkOrders.Add(workOrder);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(WorkOrderResponse.From(workOrder, customer));
    }

    private static async Task<Ok<List<WorkOrderResponse>>> ListAsync(
        WorkOrderStatus? status,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        List<WorkOrder> workOrders = await WithStatus(dbContext.WorkOrders, status)
            .OrderBy(workOrder => workOrder.PromisedOn)
            .ThenBy(workOrder => workOrder.Id)
            .ToListAsync(cancellationToken);
        Dictionary<int, Customer> customers = await LoadCustomersAsync(dbContext, workOrders, cancellationToken);

        return TypedResults.Ok(workOrders.Select(workOrder => WorkOrderResponse.From(workOrder, customers[workOrder.CustomerId])).ToList());
    }

    private static async Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> GetAsync(
        int id,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        WorkOrder? workOrder = await dbContext.WorkOrders.FindAsync([id], cancellationToken);
        if (workOrder is null)
            return UnknownJobProblem();

        Customer customer = await dbContext.Customers.SingleAsync(candidate => candidate.Id == workOrder.CustomerId, cancellationToken);
        return TypedResults.Ok(WorkOrderResponse.From(workOrder, customer));
    }

    // With no status, the board shows every open job: anything not yet collected or cancelled.
    private static IQueryable<WorkOrder> WithStatus(IQueryable<WorkOrder> workOrders, WorkOrderStatus? status) =>
        status is WorkOrderStatus wanted
            ? workOrders.Where(workOrder => workOrder.Status == wanted)
            : workOrders.Where(workOrder => workOrder.Status != WorkOrderStatus.Collected && workOrder.Status != WorkOrderStatus.Cancelled);

    private static Task<Dictionary<int, Customer>> LoadCustomersAsync(
        AppDbContext dbContext,
        List<WorkOrder> workOrders,
        CancellationToken cancellationToken)
    {
        List<int> customerIds = [.. workOrders.Select(workOrder => workOrder.CustomerId).Distinct()];
        return dbContext.Customers
            .Where(customer => customerIds.Contains(customer.Id))
            .ToDictionaryAsync(customer => customer.Id, cancellationToken);
    }

    private static WorkOrderIntake ToIntake(WorkOrderDetailsRequest request) => new(
        request.CustomerId!.Value,
        request.BikeMakeModel,
        request.BikeColour,
        request.JobType!.Value,
        request.WorkRequested,
        request.EstimatedLabourMinutes!.Value,
        request.LabourRateCentsPerHour!.Value,
        request.EstimatedPartsCents!.Value,
        request.PromisedOn!.Value,
        request.AssignedToUserId);

    private static ProblemHttpResult UnknownCustomerProblem() => TypedResults.Problem(
        statusCode: StatusCodes.Status422UnprocessableEntity,
        title: "There's no customer with that ID.");

    private static ProblemHttpResult UnknownJobProblem() => TypedResults.Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "There's no job with that number.");
}

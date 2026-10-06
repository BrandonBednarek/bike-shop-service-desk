using System.Security.Claims;

using BikeShop.Api.Domain.Customers;
using BikeShop.Api.Domain.Users;
using BikeShop.Api.Domain.WorkOrders;
using BikeShop.Api.Features.Auth;
using BikeShop.Api.Infrastructure.Persistence;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BikeShop.Api.Features.WorkOrders;

internal static class WorkOrderEndpointSupport
{
    /// <summary>
    /// Loads the job, applies one change to it and saves. Staff can't change a closed job, only the
    /// owner can. A change that breaks a rule throws a BusinessRuleException, which becomes a 422.
    /// </summary>
    public static async Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> ApplyAsync(
        int id, ClaimsPrincipal user, AppDbContext dbContext, Action<WorkOrder> change, CancellationToken cancellationToken)
    {
        WorkOrder? workOrder = await dbContext.WorkOrders.FindAsync([id], cancellationToken);
        if (workOrder is null)
            return UnknownJob();
        if (workOrder.IsClosed && !user.IsInRole(nameof(UserRole.Owner)))
            return ClosedJob();

        change(workOrder);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await OkAsync(workOrder, dbContext, cancellationToken);
    }

    public static async Task<Ok<WorkOrderResponse>> OkAsync(WorkOrder workOrder, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        Customer customer = await dbContext.Customers.SingleAsync(candidate => candidate.Id == workOrder.CustomerId, cancellationToken);
        return TypedResults.Ok(WorkOrderResponse.From(workOrder, customer));
    }

    public static ProblemHttpResult UnknownJob() => TypedResults.Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "There's no job with that number.");

    public static int CurrentUserId(ClaimsPrincipal user) => SessionClaims.ReadCurrentUser(user).Id;

    public static DateTime Now(TimeProvider clock) => clock.GetUtcNow().UtcDateTime;

    private static ProblemHttpResult ClosedJob() => TypedResults.Problem(
        statusCode: StatusCodes.Status403Forbidden,
        title: "Only the owner can change a collected or cancelled job.");
}

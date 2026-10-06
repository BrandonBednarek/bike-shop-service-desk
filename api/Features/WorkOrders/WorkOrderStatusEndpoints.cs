using System.Security.Claims;

using BikeShop.Api.Domain.Users;
using BikeShop.Api.Domain.WorkOrders;
using BikeShop.Api.Features.Auth;
using BikeShop.Api.Infrastructure.Persistence;

using Microsoft.AspNetCore.Http.HttpResults;

namespace BikeShop.Api.Features.WorkOrders;

public static class WorkOrderStatusEndpoints
{
    public static IEndpointRouteBuilder MapWorkOrderStatusEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder workOrder = endpoints.MapGroup("/api/work-orders/{id:int}").RequireAuthorization();

        workOrder.MapPost("/start", StartAsync);
        workOrder.MapPost("/hold", HoldAsync);
        workOrder.MapPost("/mark-ready", MarkReadyAsync);
        workOrder.MapPost("/collect", CollectAsync);
        workOrder.MapPost("/cancel", CancelAsync);
        workOrder.MapPost("/reopen", ReopenAsync).RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Owner)));

        return endpoints;
    }

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> StartAsync(
        int id, ClaimsPrincipal user, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ChangeAsync(id, dbContext, workOrder => workOrder.Start(CurrentUserId(user), Now(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> HoldAsync(
        int id, HoldRequest request, ClaimsPrincipal user, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ChangeAsync(id, dbContext, workOrder => HoldWithOptionalNote(workOrder, request, CurrentUserId(user), Now(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> MarkReadyAsync(
        int id, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ChangeAsync(id, dbContext, workOrder => workOrder.MarkReady(Now(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> CollectAsync(
        int id, CollectRequest request, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ChangeAsync(id, dbContext, workOrder => workOrder.Collect(request.PosReceiptNumber, Now(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> CancelAsync(
        int id, CancelRequest request, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ChangeAsync(id, dbContext, workOrder => workOrder.Cancel(request.Reason, Now(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> ReopenAsync(
        int id, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ChangeAsync(id, dbContext, workOrder => workOrder.Reopen(Now(clock)), cancellationToken);

    /// <summary>
    /// Loads the job, applies one action to it and saves. An action that breaks a rule throws a
    /// BusinessRuleException, which BusinessRuleExceptionHandler turns into a 422.
    /// </summary>
    private static async Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> ChangeAsync(
        int id, AppDbContext dbContext, Action<WorkOrder> change, CancellationToken cancellationToken)
    {
        WorkOrder? workOrder = await dbContext.WorkOrders.FindAsync([id], cancellationToken);
        if (workOrder is null)
            return WorkOrderResults.UnknownJob();

        change(workOrder);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await WorkOrderResults.OkAsync(workOrder, dbContext, cancellationToken);
    }

    private static void HoldWithOptionalNote(WorkOrder workOrder, HoldRequest request, int userId, DateTime utcNow)
    {
        workOrder.Hold(request.Reason!.Value, utcNow);
        if (!string.IsNullOrWhiteSpace(request.Note))
            workOrder.AddNote(request.Note, userId, utcNow);
    }

    private static int CurrentUserId(ClaimsPrincipal user) => SessionClaims.ReadCurrentUser(user).Id;

    private static DateTime Now(TimeProvider clock) => clock.GetUtcNow().UtcDateTime;
}

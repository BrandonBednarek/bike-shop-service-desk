using System.Security.Claims;

using BikeShop.Api.Domain.WorkOrders;
using BikeShop.Api.Infrastructure.Persistence;

using Microsoft.AspNetCore.Http.HttpResults;

using static BikeShop.Api.Features.WorkOrders.WorkOrderEndpointSupport;

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
        workOrder.MapPost("/reopen", ReopenAsync);

        return endpoints;
    }

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> StartAsync(
        int id, ClaimsPrincipal user, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ApplyAsync(id, user, dbContext, workOrder => workOrder.Start(CurrentUserId(user), UtcNow(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> HoldAsync(
        int id, HoldRequest request, ClaimsPrincipal user, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ApplyAsync(id, user, dbContext, workOrder => HoldWithOptionalNote(workOrder, request, CurrentUserId(user), UtcNow(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> MarkReadyAsync(
        int id, ClaimsPrincipal user, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ApplyAsync(id, user, dbContext, workOrder => workOrder.MarkReady(UtcNow(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> CollectAsync(
        int id, CollectRequest request, ClaimsPrincipal user, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ApplyAsync(id, user, dbContext, workOrder => workOrder.Collect(request.PosReceiptNumber, UtcNow(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> CancelAsync(
        int id, CancelRequest request, ClaimsPrincipal user, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ApplyAsync(id, user, dbContext, workOrder => workOrder.Cancel(request.Reason, UtcNow(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> ReopenAsync(
        int id, ClaimsPrincipal user, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ApplyAsync(id, user, dbContext, workOrder => workOrder.Reopen(UtcNow(clock)), cancellationToken);

    private static void HoldWithOptionalNote(WorkOrder workOrder, HoldRequest request, int userId, DateTime utcNow)
    {
        workOrder.Hold(request.Reason!.Value, utcNow);
        if (!string.IsNullOrWhiteSpace(request.Note))
            workOrder.AddNote(request.Note, userId, utcNow);
    }
}

using System.Security.Claims;

using BikeShop.Api.Infrastructure.Persistence;

using Microsoft.AspNetCore.Http.HttpResults;

using static BikeShop.Api.Features.WorkOrders.WorkOrderEndpointSupport;

namespace BikeShop.Api.Features.WorkOrders;

public static class WorkOrderLineEndpoints
{
    public static IEndpointRouteBuilder MapWorkOrderLineEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder workOrder = endpoints.MapGroup("/api/work-orders/{id:int}").RequireAuthorization();

        workOrder.MapPost("/labour", LogLabourAsync);
        workOrder.MapDelete("/labour/{entryId:int}", RemoveLabourAsync);
        workOrder.MapPost("/parts", AddPartAsync);
        workOrder.MapDelete("/parts/{partId:int}", RemovePartAsync);
        workOrder.MapPost("/notes", AddNoteAsync);

        return endpoints;
    }

    // The mechanic defaults to whoever is signed in, since they usually log their own time.
    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> LogLabourAsync(
        int id, LogLabourRequest request, ClaimsPrincipal user, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ApplyAsync(id, user, dbContext, workOrder => workOrder.LogLabour(
            request.MechanicUserId ?? CurrentUserId(user), request.Minutes!.Value, request.Note, Now(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> RemoveLabourAsync(
        int id, int entryId, ClaimsPrincipal user, AppDbContext dbContext, CancellationToken cancellationToken) =>
        ApplyAsync(id, user, dbContext, workOrder => workOrder.RemoveLabour(entryId), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> AddPartAsync(
        int id, AddPartRequest request, ClaimsPrincipal user, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ApplyAsync(id, user, dbContext, workOrder => workOrder.AddPart(
            request.Description, request.Quantity!.Value, request.UnitPriceCents!.Value, Now(clock)), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> RemovePartAsync(
        int id, int partId, ClaimsPrincipal user, AppDbContext dbContext, CancellationToken cancellationToken) =>
        ApplyAsync(id, user, dbContext, workOrder => workOrder.RemovePart(partId), cancellationToken);

    private static Task<Results<Ok<WorkOrderResponse>, ProblemHttpResult>> AddNoteAsync(
        int id, AddNoteRequest request, ClaimsPrincipal user, AppDbContext dbContext, TimeProvider clock, CancellationToken cancellationToken) =>
        ApplyAsync(id, user, dbContext, workOrder => workOrder.AddNote(request.Text, CurrentUserId(user), Now(clock)), cancellationToken);
}

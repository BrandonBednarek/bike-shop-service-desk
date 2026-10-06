namespace BikeShop.Api.Infrastructure.Errors;

public static class ErrorHandlingExtensions
{
    public static IServiceCollection AddProblemDetailsErrors(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        services.AddValidation();

        // Development defaults this to true, which turns an unreadable request body into an
        // exception that the exception handler would answer with a 500. Off, it's a 400 everywhere.
        services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);
        return services;
    }

    public static WebApplication UseProblemDetailsErrors(this WebApplication app)
    {
        // An unhandled exception is logged and answered with a 500 problem response that
        // carries a trace ID but none of the exception's details.
        app.UseExceptionHandler();

        // Fills in a problem-details body for errors that would otherwise have none,
        // such as the 401 from the sign-in check.
        app.UseStatusCodePages();
        return app;
    }

    /// <summary>
    /// Any /api address with no endpoint gets a JSON 404 instead of the app's page. The literal
    /// "api" segment outranks the app's catch-all, so the order they're mapped in doesn't matter.
    /// </summary>
    public static IEndpointRouteBuilder MapApiNotFoundFallback(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapFallback("/api/{**path}", () => TypedResults.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "There is no API endpoint at this address."));
        return endpoints;
    }
}

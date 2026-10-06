using BikeShop.Api.Domain;

using Microsoft.AspNetCore.Diagnostics;

namespace BikeShop.Api.Infrastructure.Errors;

public sealed class BusinessRuleExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Title = exception.Message },
        });
    }
}

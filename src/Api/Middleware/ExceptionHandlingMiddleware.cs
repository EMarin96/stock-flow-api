using Microsoft.AspNetCore.Diagnostics;

namespace StockFlow.Api.Middleware;

/// <summary>
/// Global exception handler: translates unhandled (truly unexpected) failures
/// into a <see cref="ProblemDetails"/> response, per the error-handling
/// convention in tech-stack.md. Expected/business errors never reach this
/// handler — they are represented as a <c>Result</c> failure instead.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    ILogger<ExceptionHandlingMiddleware> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred while processing the request.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "The request could not be completed. Please try again later.",
            },
            cancellationToken);

        return true;
    }
}

using StockFlow.Application.Common.Results;

namespace StockFlow.Api.Common;

/// <summary>
/// Translates a business <see cref="Result"/>/<see cref="Result{TValue}"/> failure
/// into an appropriate <see cref="ProblemDetails"/> HTTP response.
/// </summary>
public static class ResultExtensions
{
    public static IResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("ToProblem should only be called on a failed result.");
        }

        return Problem(result.Error);
    }

    public static IResult ToProblem<TValue>(this Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("ToProblem should only be called on a failed result.");
        }

        return Problem(result.Error);
    }

    private static IResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: statusCode);
    }
}

namespace StockFlow.Application.Common.Results;

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Failure,
}

/// <summary>
/// Represents an expected, business-level error. Used with <see cref="Result"/>
/// instead of exceptions, per the Result pattern convention in tech-stack.md —
/// exceptions are reserved for truly unexpected errors.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
}

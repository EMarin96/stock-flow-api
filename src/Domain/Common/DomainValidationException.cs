namespace StockFlow.Domain.Common;

/// <summary>
/// Thrown by Domain Value Objects when a guard clause detects an invalid value.
/// Reserved for defense-in-depth: in normal flow these are unreachable, since
/// the Application layer already validates input via FluentValidation before
/// constructing/updating a <c>Product</c> (see plan.md — Decisions).
/// </summary>
public sealed class DomainValidationException : Exception
{
    public DomainValidationException(string message)
        : base(message)
    {
    }
}

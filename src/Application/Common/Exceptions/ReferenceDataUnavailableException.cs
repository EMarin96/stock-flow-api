namespace StockFlow.Application.Common.Exceptions;

/// <summary>
/// Thrown by ICountryReferenceDataService implementations (Infrastructure) when
/// the external reference-data source cannot be reached and nothing is cached
/// yet for what was requested, so Application handlers can translate it into a
/// business <see cref="Results.Result"/> instead of letting it propagate as an
/// unhandled failure (see plan.md — Decisions).
/// </summary>
public sealed class ReferenceDataUnavailableException : Exception
{
    public ReferenceDataUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

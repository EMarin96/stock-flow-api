namespace StockFlow.Application.Common.Exceptions;

/// <summary>
/// Thrown by write repository implementations (Infrastructure) when a database
/// unique-constraint violation is detected, so Application handlers can translate
/// it into a business <see cref="Results.Result"/> without depending on the
/// underlying data-access technology (EF Core/Npgsql) directly.
/// </summary>
public sealed class DbUpdateUniqueConstraintException : Exception
{
    public DbUpdateUniqueConstraintException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

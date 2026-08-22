namespace StockFlow.Application.Common.Exceptions;

/// <summary>
/// Thrown by write repository implementations (Infrastructure) when a database
/// check-constraint violation is detected (e.g. the `CHECK (Quantity >= 0)` on
/// `StockLevels`), so Application handlers can translate it into a business
/// <see cref="Results.Result"/> without depending on the underlying data-access
/// technology (EF Core/Npgsql) directly. Mirrors
/// <see cref="DbUpdateUniqueConstraintException"/>, applied to a different
/// constraint kind (see plan.md — Implementation, step 12).
/// </summary>
public sealed class DbUpdateCheckConstraintException : Exception
{
    public DbUpdateCheckConstraintException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

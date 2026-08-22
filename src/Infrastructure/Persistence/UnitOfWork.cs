using Npgsql;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Common.Persistence;

namespace StockFlow.Infrastructure.Persistence;

public sealed class UnitOfWork(StockFlowDbContext dbContext) : IUnitOfWork
{
    private const string UniqueViolationSqlState = "23505";
    private const string CheckViolationSqlState = "23514";

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            throw new DbUpdateUniqueConstraintException("A unique constraint was violated.", exception);
        }
        catch (DbUpdateException exception) when (IsCheckConstraintViolation(exception))
        {
            // Backs the CHECK (Quantity >= 0) constraint on StockLevels — the final
            // safety net against a race a concurrent movement could slip past the
            // Application-layer guard (see plan.md — Implementation, step 12).
            throw new DbUpdateCheckConstraintException("A check constraint was violated.", exception);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: UniqueViolationSqlState };

    private static bool IsCheckConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: CheckViolationSqlState };
}

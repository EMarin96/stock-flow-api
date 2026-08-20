using Npgsql;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Common.Persistence;

namespace StockFlow.Infrastructure.Persistence;

public sealed class UnitOfWork(StockFlowDbContext dbContext) : IUnitOfWork
{
    private const string UniqueViolationSqlState = "23505";

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
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: UniqueViolationSqlState };
}

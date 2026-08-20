using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Common.Persistence;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="IUnitOfWork"/> used by handler unit tests, so
/// tests don't depend on EF Core or a live database. By default,
/// <see cref="SaveChangesAsync"/> succeeds as a no-op; pass
/// <paramref name="throwUniqueConstraintViolation"/> to simulate the race-condition
/// case where the database rejects a duplicate SKU at commit time.
/// </summary>
public sealed class InMemoryUnitOfWork(bool throwUniqueConstraintViolation = false) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        if (throwUniqueConstraintViolation)
        {
            throw new DbUpdateUniqueConstraintException(
                "A unique constraint was violated.",
                new InvalidOperationException("Simulated unique constraint violation."));
        }

        return Task.CompletedTask;
    }
}

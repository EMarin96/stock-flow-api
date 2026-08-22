using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="IStockMovementWriteRepository"/> used by
/// handler unit tests, so tests don't depend on EF Core or a live database.
/// </summary>
public sealed class InMemoryStockMovementWriteRepository : IStockMovementWriteRepository
{
    public List<StockMovement> Movements { get; } = [];

    public Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken)
    {
        Movements.Add(stockMovement);
        return Task.CompletedTask;
    }
}

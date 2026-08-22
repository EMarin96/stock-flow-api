using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="IStockLevelWriteRepository"/> used by handler
/// unit tests. To seed an existing quantity, create a <see cref="StockLevel"/>
/// and call <see cref="StockLevel.Increase"/> before passing it in — mirrors
/// the only way Quantity can change in production code.
/// </summary>
public sealed class InMemoryStockLevelWriteRepository : IStockLevelWriteRepository
{
    private readonly List<StockLevel> _stockLevels;

    public InMemoryStockLevelWriteRepository(IEnumerable<StockLevel>? seed = null)
    {
        _stockLevels = seed?.ToList() ?? [];
    }

    public Task<StockLevel> GetOrCreateAsync(Guid productId, Guid locationId, CancellationToken cancellationToken)
    {
        var stockLevel = _stockLevels.FirstOrDefault(level => level.ProductId == productId && level.LocationId == locationId);
        if (stockLevel is not null)
        {
            return Task.FromResult(stockLevel);
        }

        stockLevel = StockLevel.Create(productId, locationId);
        _stockLevels.Add(stockLevel);

        return Task.FromResult(stockLevel);
    }
}

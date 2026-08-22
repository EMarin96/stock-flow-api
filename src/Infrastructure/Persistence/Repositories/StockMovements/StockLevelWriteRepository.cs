using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;
using StockFlow.Infrastructure.Persistence;

namespace StockFlow.Infrastructure.Persistence.Repositories.StockMovements;

public sealed class StockLevelWriteRepository(StockFlowDbContext dbContext) : IStockLevelWriteRepository
{
    /// <summary>
    /// If no row exists yet for this product+location pair, a fresh one is
    /// tracked (added, at Quantity = 0) but not yet persisted — the caller
    /// mutates it via Increase/Decrease and it is inserted alongside the
    /// StockMovement in the same SaveChangesAsync call (see plan.md — Approach).
    /// A race with a concurrent first movement for the same pair is caught by
    /// the composite unique index on (ProductId, LocationId) (see plan.md — Risks).
    /// </summary>
    public async Task<StockLevel> GetOrCreateAsync(Guid productId, Guid locationId, CancellationToken cancellationToken)
    {
        var stockLevel = await dbContext.StockLevels.FirstOrDefaultAsync(
            level => level.ProductId == productId && level.LocationId == locationId,
            cancellationToken);

        if (stockLevel is not null)
        {
            return stockLevel;
        }

        stockLevel = StockLevel.Create(productId, locationId);
        await dbContext.StockLevels.AddAsync(stockLevel, cancellationToken);

        return stockLevel;
    }
}

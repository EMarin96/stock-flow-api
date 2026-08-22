using StockFlow.Domain.StockMovements;

namespace StockFlow.Application.StockMovements.Shared;

/// <summary>
/// Write-side stock level access, backed by EF Core. Used by
/// CreateStockMovementHandler to load (or create, for the product+location
/// pair's first-ever movement) the tracked <see cref="StockLevel"/> row it then
/// mutates via Increase/Decrease within the same SaveChangesAsync as the
/// movement insert (see plan.md — Approach).
/// </summary>
public interface IStockLevelWriteRepository
{
    Task<StockLevel> GetOrCreateAsync(Guid productId, Guid locationId, CancellationToken cancellationToken);
}

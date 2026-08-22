namespace StockFlow.Application.StockMovements.Shared;

/// <summary>
/// Read-side stock level access, backed by Dapper. Used by
/// GetLocationStockHandler.
/// </summary>
public interface IStockLevelReadRepository
{
    /// <summary>
    /// Returns every active product with its current stock at
    /// <paramref name="locationId"/>, including products with no
    /// <c>StockLevel</c> row yet (surfaced as <c>Quantity = 0</c>) — see
    /// plan.md ("returns every StockLevel row for that location — including
    /// Quantity = 0").
    /// </summary>
    Task<PagedResult<LocationStockDto>> GetPagedByLocationAsync(
        Guid locationId,
        string? productNameFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

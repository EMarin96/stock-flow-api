using StockFlow.Domain.StockMovements;

namespace StockFlow.Application.StockMovements.Shared;

/// <summary>
/// Read-side stock movement access, backed by Dapper. Used by
/// GetStockMovementsHandler.
/// </summary>
public interface IStockMovementReadRepository
{
    /// <summary>
    /// <paramref name="locationId"/>, when supplied, matches either
    /// SourceLocationId or DestinationLocationId (see plan.md — Decisions).
    /// </summary>
    Task<PagedResult<StockMovementDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? productId,
        Guid? locationId,
        MovementType? type,
        CancellationToken cancellationToken);
}

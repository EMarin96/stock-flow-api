using StockFlow.Application.Common.Pagination;
using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="IStockMovementReadRepository"/> used by
/// handler unit tests. Mirrors the Dapper implementation's filtering, including
/// LocationId matching either SourceLocationId or DestinationLocationId.
/// </summary>
public sealed class InMemoryStockMovementReadRepository(IEnumerable<StockMovementDto> seed) : IStockMovementReadRepository
{
    private readonly List<StockMovementDto> _movements = seed.ToList();

    public Task<PagedResult<StockMovementDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? productId,
        Guid? locationId,
        MovementType? type,
        CancellationToken cancellationToken)
    {
        var filtered = _movements
            .Where(movement => productId is null || movement.ProductId == productId)
            .Where(movement => locationId is null || movement.SourceLocationId == locationId || movement.DestinationLocationId == locationId)
            .Where(movement => type is null || movement.Type == type)
            .ToList();

        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<StockMovementDto>(items, page, pageSize, filtered.Count));
    }
}

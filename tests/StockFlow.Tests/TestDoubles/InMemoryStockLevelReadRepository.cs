using StockFlow.Application.Common.Pagination;
using StockFlow.Application.StockMovements.Shared;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="IStockLevelReadRepository"/> used by
/// handler unit tests.
/// </summary>
public sealed class InMemoryStockLevelReadRepository(IEnumerable<LocationStockDto> seed) : IStockLevelReadRepository
{
    private readonly List<LocationStockDto> _stock = seed.ToList();

    public Task<PagedResult<LocationStockDto>> GetPagedByLocationAsync(
        Guid locationId,
        string? productNameFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var filtered = _stock
            .Where(item => productNameFilter is null || item.ProductName.Contains(productNameFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<LocationStockDto>(items, page, pageSize, filtered.Count));
    }
}

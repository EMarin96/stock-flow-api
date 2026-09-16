using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Reporting.Shared;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="IReportingReadRepository"/> used by
/// handler unit tests. Stock overview is seeded pre-aggregated (mirrors
/// <see cref="InMemoryStockLevelReadRepository"/>'s convention of seeding
/// already-computed rows); movement activity is seeded as raw per-movement
/// rows and grouped by (ProductId, Type) in <see cref="GetMovementActivityAsync"/>,
/// mirroring the real SQL's GROUP BY (see plan.md — Implementation).
/// </summary>
public sealed class InMemoryReportingReadRepository(
    IEnumerable<StockOverviewDto>? stockOverviewSeed = null,
    IEnumerable<InMemoryReportingReadRepository.MovementSeed>? movementSeed = null) : IReportingReadRepository
{
    private readonly List<StockOverviewDto> _stockOverview = stockOverviewSeed?.ToList() ?? [];
    private readonly List<MovementSeed> _movements = movementSeed?.ToList() ?? [];

    public sealed record MovementSeed(
        Guid ProductId,
        string ProductSku,
        string ProductName,
        MovementType Type,
        int Quantity,
        Guid? SourceLocationId,
        Guid? DestinationLocationId,
        DateTime CreatedAt);

    public Task<PagedResult<StockOverviewDto>> GetStockOverviewAsync(
        int page,
        int pageSize,
        bool? lowStockOnly,
        CancellationToken cancellationToken)
    {
        var filtered = _stockOverview
            .Where(row => lowStockOnly is not true || row.IsLowStock)
            .ToList();

        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<StockOverviewDto>(items, page, pageSize, filtered.Count));
    }

    public Task<PagedResult<MovementActivityDto>> GetMovementActivityAsync(
        DateTime? from,
        DateTime? to,
        Guid? productId,
        Guid? locationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var grouped = _movements
            .Where(movement => from is null || movement.CreatedAt >= from)
            .Where(movement => to is null || movement.CreatedAt <= to)
            .Where(movement => productId is null || movement.ProductId == productId)
            .Where(movement => locationId is null
                || movement.SourceLocationId == locationId
                || movement.DestinationLocationId == locationId)
            .GroupBy(movement => (movement.ProductId, movement.Type))
            .Select(group => new MovementActivityDto(
                group.Key.ProductId,
                group.First().ProductSku,
                group.First().ProductName,
                group.Key.Type,
                group.Count(),
                group.Sum(movement => movement.Quantity)))
            .OrderBy(dto => dto.ProductName)
            .ThenBy(dto => dto.Type)
            .ToList();

        var items = grouped
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<MovementActivityDto>(items, page, pageSize, grouped.Count));
    }
}

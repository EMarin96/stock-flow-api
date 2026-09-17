namespace StockFlow.Application.Reporting.Shared;

/// <summary>
/// Read-side access for both reporting endpoints, backed by Dapper. One
/// repository for the whole feature, several methods — same convention
/// already used by <c>ILocationReadRepository</c> (see plan.md — Decisions).
/// </summary>
public interface IReportingReadRepository
{
    Task<PagedResult<StockOverviewDto>> GetStockOverviewAsync(
        int page,
        int pageSize,
        bool? lowStockOnly,
        CancellationToken cancellationToken);

    /// <summary>
    /// <paramref name="locationId"/>, when supplied, matches either a
    /// movement's SourceLocationId or DestinationLocationId — same convention
    /// as <c>IStockMovementReadRepository.GetPagedAsync</c>.
    /// </summary>
    Task<PagedResult<MovementActivityDto>> GetMovementActivityAsync(
        DateTime? from,
        DateTime? to,
        Guid? productId,
        Guid? locationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

using StockFlow.Application.Reporting.Shared;

namespace StockFlow.Application.Reporting.GetStockOverview;

public sealed record GetStockOverviewQuery(
    int Page,
    int PageSize,
    bool? LowStockOnly) : IRequest<Result<PagedResult<StockOverviewDto>>>;

using StockFlow.Application.StockMovements.Shared;

namespace StockFlow.Application.StockMovements.GetLocationStock;

public sealed record GetLocationStockQuery(
    Guid LocationId,
    string? ProductNameFilter,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<LocationStockDto>>>;

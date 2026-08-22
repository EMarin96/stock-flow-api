using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Application.StockMovements.GetStockMovements;

/// <summary>
/// <paramref name="LocationId"/>, when supplied, matches either
/// SourceLocationId or DestinationLocationId (see plan.md — Decisions).
/// </summary>
public sealed record GetStockMovementsQuery(
    int Page,
    int PageSize,
    Guid? ProductId,
    Guid? LocationId,
    MovementType? Type) : IRequest<Result<PagedResult<StockMovementDto>>>;

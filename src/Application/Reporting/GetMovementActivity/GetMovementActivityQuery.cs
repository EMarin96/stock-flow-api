using StockFlow.Application.Reporting.Shared;

namespace StockFlow.Application.Reporting.GetMovementActivity;

/// <summary>
/// <paramref name="LocationId"/>, when supplied, matches either a movement's
/// SourceLocationId or DestinationLocationId (see plan.md — Decisions).
/// </summary>
public sealed record GetMovementActivityQuery(
    DateTime? From,
    DateTime? To,
    Guid? ProductId,
    Guid? LocationId,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<MovementActivityDto>>>;

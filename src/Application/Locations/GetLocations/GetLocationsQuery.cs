using StockFlow.Application.Locations.Shared;

namespace StockFlow.Application.Locations.GetLocations;

public sealed record GetLocationsQuery(int Page, int PageSize) : IRequest<Result<PagedResult<LocationDto>>>;

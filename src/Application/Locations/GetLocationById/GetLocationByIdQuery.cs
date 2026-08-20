using StockFlow.Application.Locations.Shared;

namespace StockFlow.Application.Locations.GetLocationById;

public sealed record GetLocationByIdQuery(Guid Id) : IRequest<Result<LocationDto>>;

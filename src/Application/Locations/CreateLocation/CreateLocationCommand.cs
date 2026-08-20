using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Application.Locations.CreateLocation;

public sealed record CreateLocationCommand(
    string Code,
    string Name,
    string? AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string State,
    string City,
    Country Country) : IRequest<Result<LocationDto>>;

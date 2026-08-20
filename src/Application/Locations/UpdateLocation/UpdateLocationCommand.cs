using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Application.Locations.UpdateLocation;

// The code is intentionally not part of this command — it is immutable once created.
public sealed record UpdateLocationCommand(
    Guid Id,
    string Name,
    string? AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string State,
    string City,
    Country Country) : IRequest<Result<LocationDto>>;

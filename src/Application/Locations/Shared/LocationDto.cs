using StockFlow.Domain.Locations;

namespace StockFlow.Application.Locations.Shared;

public sealed record LocationDto(
    Guid Id,
    string Code,
    string Name,
    string? AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string State,
    string City,
    Country Country,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy);

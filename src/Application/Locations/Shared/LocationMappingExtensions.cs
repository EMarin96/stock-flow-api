using StockFlow.Domain.Locations;

namespace StockFlow.Application.Locations.Shared;

public static class LocationMappingExtensions
{
    public static LocationDto ToDto(this Location location) => new(
        location.Id,
        location.Code.Value,
        location.Name,
        location.Address.AddressLine1,
        location.Address.AddressLine2,
        location.Address.AddressLine3,
        location.Address.State,
        location.Address.City,
        location.Address.Country,
        location.CreatedAt,
        location.CreatedBy,
        location.UpdatedAt,
        location.UpdatedBy);
}

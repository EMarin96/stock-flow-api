using StockFlow.Application.Common.Exceptions;
using StockFlow.Domain.Locations;

namespace StockFlow.Application.Locations.Shared;

/// <summary>
/// Shared reference-data validation for the State/City core trio, used by both
/// CreateLocationHandler and UpdateLocationHandler. Domain can only validate
/// the trio's shape; confirming State/City are *real* values for the given
/// Country requires I/O, so it lives here at the Application boundary
/// (see plan.md — Decisions).
/// </summary>
public static class AddressReferenceDataValidator
{
    public static async Task<Error?> ValidateAsync(
        ICountryReferenceDataService referenceDataService,
        Country country,
        string state,
        string city,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StateInfo> states;
        try
        {
            states = await referenceDataService.GetStatesAsync(country, cancellationToken);
        }
        catch (ReferenceDataUnavailableException)
        {
            return LocationErrors.ReferenceDataUnavailable();
        }

        var matchedState = states.FirstOrDefault(candidate => string.Equals(candidate.Iso2, state, StringComparison.OrdinalIgnoreCase));
        if (matchedState is null)
        {
            return LocationErrors.InvalidState(country, state);
        }

        IReadOnlyList<CityInfo> cities;
        try
        {
            cities = await referenceDataService.GetCitiesAsync(country, matchedState.Iso2, cancellationToken);
        }
        catch (ReferenceDataUnavailableException)
        {
            return LocationErrors.ReferenceDataUnavailable();
        }

        var cityIsValid = cities.Any(candidate => string.Equals(candidate.Name, city, StringComparison.OrdinalIgnoreCase));

        return cityIsValid ? null : LocationErrors.InvalidCity(country, state, city);
    }
}

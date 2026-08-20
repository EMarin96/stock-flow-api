using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// Test double for <see cref="ICountryReferenceDataService"/> that always throws
/// <see cref="ReferenceDataUnavailableException"/>, simulating the external
/// reference-data API being unreachable with nothing cached yet.
/// </summary>
public sealed class UnavailableCountryReferenceDataService : ICountryReferenceDataService
{
    public Task<IReadOnlyList<StateInfo>> GetStatesAsync(Country country, CancellationToken cancellationToken) =>
        throw new ReferenceDataUnavailableException($"Reference data is unavailable for country '{country}'.");

    public Task<IReadOnlyList<CityInfo>> GetCitiesAsync(Country country, string stateIso2, CancellationToken cancellationToken) =>
        throw new ReferenceDataUnavailableException($"Reference data is unavailable for state '{stateIso2}' in country '{country}'.");
}

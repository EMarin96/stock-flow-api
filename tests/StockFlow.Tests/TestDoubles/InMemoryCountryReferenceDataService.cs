using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// Fixed, hardcoded fake for <see cref="ICountryReferenceDataService"/>, used by
/// both unit and integration tests so tests never call the real
/// countrystatecity.in API (see plan.md — Decisions). Seeded with enough
/// "known good" and "known bad" state/city combinations to exercise both
/// validation success and failure paths.
/// </summary>
public sealed class InMemoryCountryReferenceDataService : ICountryReferenceDataService
{
    private static readonly Dictionary<Country, IReadOnlyList<StateInfo>> States = new()
    {
        [Country.US] = new List<StateInfo>
        {
            new("CA", "California"),
            new("NY", "New York"),
        },
        [Country.CR] = new List<StateInfo>
        {
            new("SJ", "San José"),
            new("A", "Alajuela"),
        },
    };

    private static readonly Dictionary<(Country Country, string StateIso2), IReadOnlyList<CityInfo>> Cities = new()
    {
        [(Country.US, "CA")] = new List<CityInfo> { new("Los Angeles"), new("San Francisco") },
        [(Country.US, "NY")] = new List<CityInfo> { new("New York City"), new("Albany") },
        [(Country.CR, "SJ")] = new List<CityInfo> { new("San José"), new("Escazú") },
        [(Country.CR, "A")] = new List<CityInfo> { new("Alajuela"), new("San Ramón") },
    };

    public Task<IReadOnlyList<StateInfo>> GetStatesAsync(Country country, CancellationToken cancellationToken) =>
        Task.FromResult(States.TryGetValue(country, out var states) ? states : (IReadOnlyList<StateInfo>)[]);

    public Task<IReadOnlyList<CityInfo>> GetCitiesAsync(Country country, string stateIso2, CancellationToken cancellationToken) =>
        Task.FromResult(
            Cities.TryGetValue((country, stateIso2.ToUpperInvariant()), out var cities) ? cities : (IReadOnlyList<CityInfo>)[]);
}

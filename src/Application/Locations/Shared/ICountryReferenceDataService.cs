using StockFlow.Domain.Locations;

namespace StockFlow.Application.Locations.Shared;

/// <summary>
/// Looks up real, recognized states/cities for a given <see cref="Country"/>,
/// used to validate a Location's Address.State/Address.City beyond the
/// Domain's shape-only guard clause (see plan.md — Decisions). Implemented in
/// Infrastructure against the external countrystatecity.in API; a fixed,
/// hardcoded fake is used in tests so they never call the real API.
/// </summary>
public interface ICountryReferenceDataService
{
    Task<IReadOnlyList<StateInfo>> GetStatesAsync(Country country, CancellationToken cancellationToken);

    /// <summary>
    /// Cities are looked up by state ISO2 code but matched/stored by name only —
    /// the external API's Basic tier only returns id+name for cities, and using
    /// a third-party numeric id would be opaque without cross-referencing that
    /// provider (see plan.md — Decisions, "City is always identified/stored by
    /// name, never by the external API's numeric id").
    /// </summary>
    Task<IReadOnlyList<CityInfo>> GetCitiesAsync(Country country, string stateIso2, CancellationToken cancellationToken);
}

public sealed record StateInfo(string Iso2, string Name);

public sealed record CityInfo(string Name);

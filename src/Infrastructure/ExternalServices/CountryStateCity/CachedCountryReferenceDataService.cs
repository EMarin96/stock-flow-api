using Microsoft.Extensions.Caching.Memory;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Infrastructure.ExternalServices.CountryStateCity;

/// <summary>
/// Decorates <see cref="CountryStateCityApiClient"/> with an
/// <see cref="IMemoryCache"/> layer: states are cached per <see cref="Country"/>
/// with no expiration (warmed up eagerly at startup for US/CR — see
/// StatesCacheWarmupHostedService in the Api project), cities are cached per
/// (Country, State) with no expiration, populated lazily on first request for
/// that pair (see plan.md — Decisions). If the external API is unreachable and
/// nothing is cached yet for what was requested, throws
/// <see cref="ReferenceDataUnavailableException"/> rather than silently
/// returning stale/empty data.
/// </summary>
public sealed class CachedCountryReferenceDataService(
    CountryStateCityApiClient apiClient,
    IMemoryCache cache) : ICountryReferenceDataService
{
    public async Task<IReadOnlyList<StateInfo>> GetStatesAsync(Country country, CancellationToken cancellationToken)
    {
        var cacheKey = StatesCacheKey(country);

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<StateInfo>? cachedStates) && cachedStates is not null)
        {
            return cachedStates;
        }

        IReadOnlyList<CountryStateCityStateResponse> response;
        try
        {
            response = await apiClient.GetStatesAsync(country.ToString(), cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new ReferenceDataUnavailableException(
                $"Could not fetch states for country '{country}' from the reference-data API.", exception);
        }

        var states = (IReadOnlyList<StateInfo>)response.Select(state => new StateInfo(state.Iso2, state.Name)).ToList();
        cache.Set(cacheKey, states);
        return states;
    }

    public async Task<IReadOnlyList<CityInfo>> GetCitiesAsync(Country country, string stateIso2, CancellationToken cancellationToken)
    {
        var cacheKey = CitiesCacheKey(country, stateIso2);

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<CityInfo>? cachedCities) && cachedCities is not null)
        {
            return cachedCities;
        }

        IReadOnlyList<CountryStateCityCityResponse> response;
        try
        {
            response = await apiClient.GetCitiesAsync(country.ToString(), stateIso2, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new ReferenceDataUnavailableException(
                $"Could not fetch cities for state '{stateIso2}' in country '{country}' from the reference-data API.", exception);
        }

        // Cities are matched/stored by name only, never by the external API's
        // numeric id — a permanent decision, not just for this iteration
        // (see plan.md — Decisions).
        var cities = (IReadOnlyList<CityInfo>)response.Select(city => new CityInfo(city.Name)).ToList();
        cache.Set(cacheKey, cities);
        return cities;
    }

    private static string StatesCacheKey(Country country) => $"CountryStateCity:States:{country}";

    private static string CitiesCacheKey(Country country, string stateIso2) =>
        $"CountryStateCity:Cities:{country}:{stateIso2.ToUpperInvariant()}";
}

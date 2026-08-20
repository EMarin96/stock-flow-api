using System.Net.Http.Json;
using System.Text.Json;

namespace StockFlow.Infrastructure.ExternalServices.CountryStateCity;

/// <summary>
/// Thin typed-HttpClient wrapper around the countrystatecity.in API's verified
/// endpoints (see plan.md — Implementation): <c>GET /v1/countries/{iso2}/states</c>
/// and <c>GET /v1/countries/{countryIso2}/states/{stateIso2}/cities</c>. The
/// HttpClient's BaseAddress and the X-CSCAPI-KEY header are configured at
/// registration time (see Program.cs), not here.
/// </summary>
public sealed class CountryStateCityApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<CountryStateCityStateResponse>> GetStatesAsync(string countryIso2, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"countries/{countryIso2}/states", cancellationToken);
        response.EnsureSuccessStatusCode();

        var states = await response.Content.ReadFromJsonAsync<List<CountryStateCityStateResponse>>(JsonOptions, cancellationToken);
        return states ?? [];
    }

    /// <summary>
    /// Basic tier of the API returns only id+name for cities — deserialized here
    /// via <see cref="CountryStateCityCityResponse"/>, which intentionally has no
    /// id property (see plan.md — Decisions, "City is always identified/stored by
    /// name, never by the external API's numeric id").
    /// </summary>
    public async Task<IReadOnlyList<CountryStateCityCityResponse>> GetCitiesAsync(
        string countryIso2,
        string stateIso2,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"countries/{countryIso2}/states/{stateIso2}/cities", cancellationToken);
        response.EnsureSuccessStatusCode();

        var cities = await response.Content.ReadFromJsonAsync<List<CountryStateCityCityResponse>>(JsonOptions, cancellationToken);
        return cities ?? [];
    }
}

public sealed record CountryStateCityStateResponse(string Iso2, string Name);

public sealed record CountryStateCityCityResponse(string Name);

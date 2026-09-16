using StockFlow.Domain.Locations;

namespace StockFlow.Application.Countries.Shared;

/// <summary>
/// Case-insensitive parsing of a country code (e.g. "us", "US") into the
/// closed <see cref="Country"/> enum, shared by GetStates/GetCities so the
/// parsing rule lives in one place (see plan.md — Risks).
/// </summary>
public static class CountryCodeParser
{
    public static bool TryParse(string code, out Country country) =>
        Enum.TryParse(code, ignoreCase: true, out country) && Enum.IsDefined(country);
}

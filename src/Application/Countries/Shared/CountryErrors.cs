using StockFlow.Domain.Locations;

namespace StockFlow.Application.Countries.Shared;

public static class CountryErrors
{
    public static Error InvalidCountryCode(string code) =>
        Error.Validation(
            "Countries.InvalidCountryCode",
            $"'{code}' is not a country StockFlow currently supports.");

    public static Error InvalidState(Country country, string state) =>
        Error.Validation(
            "Countries.InvalidState",
            $"'{state}' is not a recognized state/province for country '{country}'.");

    public static Error ReferenceDataUnavailable() =>
        Error.Unavailable(
            "Countries.ReferenceDataUnavailable",
            "The country/state/city reference-data service is currently unavailable. Please try again later.");
}

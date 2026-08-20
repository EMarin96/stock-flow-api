namespace StockFlow.Infrastructure.ExternalServices.CountryStateCity;

/// <summary>
/// Options Pattern configuration for the countrystatecity.in external
/// reference-data API (see plan.md — Implementation), bound from the
/// "CountryStateCity" configuration section.
/// </summary>
/// <remarks>
/// <see cref="ApiKey"/> is a genuine secret, unlike the local Postgres
/// password already checked into appsettings.Development.json — it must never
/// be committed. Set it locally via
/// <c>dotnet user-secrets set "CountryStateCity:ApiKey" "&lt;key&gt;"</c>
/// (run from src/Api), or via a CI/deployment secret in other environments.
/// </remarks>
public sealed class CountryStateCityOptions
{
    public const string SectionName = "CountryStateCity";

    public string BaseUrl { get; set; } = "https://api.countrystatecity.in/v1/";

    public string ApiKey { get; set; } = string.Empty;
}

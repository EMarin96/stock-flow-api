using Microsoft.Extensions.Options;
using StockFlow.Api.HostedServices;
using StockFlow.Infrastructure.ExternalServices.CountryStateCity;

namespace StockFlow.Api.Configuration;

public static class CountryStateCityServiceCollectionExtensions
{
    /// <summary>
    /// Address State/City reference-data lookups (see plan.md — Implementation):
    /// IMemoryCache backs CachedCountryReferenceDataService (registered in
    /// AddInfrastructure); the typed HttpClient talks to the external
    /// countrystatecity.in API; the hosted service warms the states cache for
    /// every supported Country at startup without blocking/failing startup.
    /// </summary>
    public static IServiceCollection AddCountryStateCityIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<CountryStateCityOptions>(configuration.GetSection(CountryStateCityOptions.SectionName));
        services.AddHttpClient<CountryStateCityApiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CountryStateCityOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", options.ApiKey);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddHostedService<StatesCacheWarmupHostedService>();

        return services;
    }
}

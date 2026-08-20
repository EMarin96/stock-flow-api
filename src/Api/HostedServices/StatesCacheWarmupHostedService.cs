using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Api.HostedServices;

/// <summary>
/// Eagerly warms the states cache for every supported <see cref="Country"/> at
/// startup, so the first real request that needs address validation never pays
/// that latency (cities remain lazy — see plan.md — Decisions). Deliberately
/// does not block or fail app startup if the external reference-data API is
/// unreachable: it logs a warning and lets the first real request retry the
/// fetch on demand.
/// </summary>
public sealed class StatesCacheWarmupHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<StatesCacheWarmupHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var referenceDataService = scope.ServiceProvider.GetRequiredService<ICountryReferenceDataService>();

        foreach (var country in Enum.GetValues<Country>())
        {
            try
            {
                await referenceDataService.GetStatesAsync(country, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Could not warm up the states cache for country '{Country}' at startup; it will be fetched lazily on first use.",
                    country);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

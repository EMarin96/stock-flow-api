using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StockFlow.Application.Locations.Shared;
using StockFlow.Infrastructure.Persistence;
using StockFlow.Infrastructure.Persistence.Read;

namespace StockFlow.Tests.Api.Locations;

/// <summary>
/// Boots the API against a disposable Postgres database provided by a
/// Testcontainers container (see <see cref="PostgresContainerFixture"/>) and
/// applies migrations before the first request, per the integration-test
/// convention in tasks.md.
/// </summary>
public sealed class LocationApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public LocationApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<StockFlowDbContext>>();
            services.RemoveAll<DbContextOptions>();

            services.AddDbContext<StockFlowDbContext>(options => options.UseNpgsql(_connectionString));

            // AddInfrastructure also registers ISqlConnectionFactory (Dapper reads) with a
            // connection string baked in at startup, independently of the DbContext above —
            // it must be swapped too, or read queries would silently keep hitting the dev DB.
            services.RemoveAll<ISqlConnectionFactory>();
            services.AddScoped<ISqlConnectionFactory>(_ => new NpgsqlConnectionFactory(_connectionString));

            // Tests must never call the real countrystatecity.in API — swap in a
            // fixed, hardcoded fake (see plan.md — Decisions).
            services.RemoveAll<ICountryReferenceDataService>();
            services.AddSingleton<ICountryReferenceDataService, InMemoryCountryReferenceDataService>();
        });

        base.ConfigureWebHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StockFlowDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StockFlowDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync("""TRUNCATE TABLE "Locations";""");
    }
}

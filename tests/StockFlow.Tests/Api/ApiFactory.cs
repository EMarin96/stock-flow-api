using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StockFlow.Application.Common.Security;
using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Users;
using StockFlow.Infrastructure.Persistence;
using StockFlow.Infrastructure.Persistence.Dapper;
using StockFlow.Tests;

namespace StockFlow.Tests.Api;

/// <summary>
/// Boots the API against a disposable Postgres database provided by a
/// Testcontainers container (see <see cref="PostgresContainerFixture"/>) and
/// applies migrations before the first request, per the integration-test
/// convention in tasks.md. Shared across every feature's API tests — the DI
/// swaps and reset logic below are identical regardless of which feature's
/// endpoints a given test class exercises.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Deterministic test-only Jwt/Seed configuration — the startup
        // admin-bootstrap block (Program.cs) and JWT validation run
        // unconditionally, so tests must never depend on user-secrets/env vars
        // that may or may not be present on a given machine/CI runner (see
        // tasks.md — "extend ApiFactory/ApiFactoryFixture with a way to obtain
        // a bearer token per role").
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-only-signing-key-do-not-use-in-production-environments-32+chars",
                ["Jwt:Issuer"] = "StockFlowApi",
                ["Jwt:Audience"] = "StockFlowApi",
                ["Jwt:ExpiryMinutes"] = "480",
                ["Seed:AdminUsername"] = "seed-admin",
                ["Seed:AdminPassword"] = "Seed-Admin-Password-1!",
            });
        });

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
            // fixed, hardcoded fake (see 002's plan.md — Decisions). Registered
            // unconditionally: harmless no-op for tests that never touch Locations.
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

    /// <summary>
    /// Truncates every table any feature's API tests can write to, in a single
    /// statement — Postgres requires all FK-related tables to be truncated
    /// together (StockLevels/StockMovements both FK to Products and Locations).
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StockFlowDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync(TestDatabase.TruncateAllTablesSql);
    }

    /// <summary>
    /// Mints a JWT directly via the test host's <see cref="IJwtTokenGenerator"/>
    /// DI registration, rather than seeding a real user and calling
    /// POST /api/auth/login (see tasks.md — "extend ApiFactory/ApiFactoryFixture
    /// with a way to obtain a bearer token per role"). Valid because JWT
    /// validation is purely signature/claims-based — it never looks the user
    /// up in the database — so the subject id doesn't need to correspond to an
    /// existing row.
    /// </summary>
    public string CreateToken(Role role, Guid? userId = null, string? username = null)
    {
        using var scope = Services.CreateScope();
        var jwtTokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var (token, _) = jwtTokenGenerator.GenerateToken(userId ?? Guid.NewGuid(), username ?? $"test-{role}".ToLowerInvariant(), role);
        return token;
    }

    /// <summary>
    /// An <see cref="HttpClient"/> pre-configured with a bearer token for the
    /// given role — the standard way test classes exercise authorized
    /// requests.
    /// </summary>
    public HttpClient CreateAuthorizedClient(Role role, Guid? userId = null, string? username = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(role, userId, username));
        return client;
    }
}

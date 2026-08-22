using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockFlow.Application.Common.Persistence;
using StockFlow.Application.Locations.Shared;
using StockFlow.Application.Products.Shared;
using StockFlow.Application.StockMovements.Shared;
using StockFlow.Infrastructure.ExternalServices.CountryStateCity;
using StockFlow.Infrastructure.Persistence;
using StockFlow.Infrastructure.Persistence.Repositories.Locations;
using StockFlow.Infrastructure.Persistence.Repositories.Products;
using StockFlow.Infrastructure.Persistence.Repositories.StockMovements;

namespace StockFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StockFlowDatabase")
            ?? throw new InvalidOperationException("Connection string 'StockFlowDatabase' was not found.");

        services.AddDbContext<StockFlowDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ISqlConnectionFactory>(_ => new NpgsqlConnectionFactory(connectionString));

        services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<ILocationWriteRepository, LocationWriteRepository>();
        services.AddScoped<ILocationReadRepository, LocationReadRepository>();
        services.AddScoped<IStockMovementWriteRepository, StockMovementWriteRepository>();
        services.AddScoped<IStockMovementReadRepository, StockMovementReadRepository>();
        services.AddScoped<IStockLevelWriteRepository, StockLevelWriteRepository>();
        services.AddScoped<IStockLevelReadRepository, StockLevelReadRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // CountryStateCityApiClient (typed HttpClient), IMemoryCache, and the
        // states-cache warm-up hosted service are registered in Program.cs, since
        // AddHttpClient/AddMemoryCache/AddHostedService are Api-layer (ASP.NET
        // Core host) concerns (see plan.md — Implementation).
        services.AddScoped<ICountryReferenceDataService, CachedCountryReferenceDataService>();

        return services;
    }
}

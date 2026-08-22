using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StockFlow.Api.Endpoints;
using StockFlow.Api.HostedServices;
using StockFlow.Api.Middleware;
using StockFlow.Application;
using StockFlow.Infrastructure;
using StockFlow.Infrastructure.ExternalServices.CountryStateCity;
using StockFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();
builder.Services.AddProblemDetails();

// Address State/City reference-data lookups (see plan.md — Implementation):
// IMemoryCache backs CachedCountryReferenceDataService (registered in
// AddInfrastructure); the typed HttpClient talks to the external
// countrystatecity.in API; the hosted service warms the states cache for
// every supported Country at startup without blocking/failing startup.
builder.Services.AddMemoryCache();
builder.Services.Configure<CountryStateCityOptions>(builder.Configuration.GetSection(CountryStateCityOptions.SectionName));
builder.Services.AddHttpClient<CountryStateCityApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<CountryStateCityOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", options.ApiKey);
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHostedService<StatesCacheWarmupHostedService>();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "StockFlow API",
        Version = "v1",
        Description = "Inventory management API — product catalog, stock movements, and locations.",
    });

    // Swashbuckle's schema generator does not automatically honor the
    // JsonStringEnumConverter registered above for minimal APIs, so enums
    // (e.g. Currency) render as raw integers unless told otherwise here.
    options.SchemaFilter<EnumSchemaFilter>();
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Local dev convenience only — applies any pending EF Core migrations on
    // startup so `dotnet run` works without a separate `dotnet ef database
    // update` step. Never runs outside Development; real environments apply
    // migrations as an explicit, controlled deploy step.
    using var migrationScope = app.Services.CreateScope();
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<StockFlowDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapProductEndpoints();
app.MapLocationEndpoints();
app.MapStockMovementEndpoints();

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program
{
}

/// <summary>
/// Renders enum schemas (e.g. Currency) as their string member names in
/// Swagger, matching the wire format produced by the JsonStringEnumConverter
/// registered for requests/responses.
/// </summary>
internal sealed class EnumSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(Microsoft.OpenApi.IOpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
    {
        if (!context.Type.IsEnum || schema is not Microsoft.OpenApi.OpenApiSchema concreteSchema)
        {
            return;
        }

        concreteSchema.Type = Microsoft.OpenApi.JsonSchemaType.String;
        concreteSchema.Format = null;
        concreteSchema.Enum = [.. Enum.GetNames(context.Type).Select(name => (System.Text.Json.Nodes.JsonNode)name)];
    }
}

using StockFlow.Api.Configuration;
using StockFlow.Api.Endpoints;
using StockFlow.Api.Middleware;
using StockFlow.Api.Security;
using StockFlow.Api.Startup;
using StockFlow.Application;
using StockFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();
builder.Services.AddProblemDetails();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRoleAuthorization();

builder.Services.AddCountryStateCityIntegration(builder.Configuration);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddApiDocumentation();

var app = builder.Build();

app.UseExceptionHandler();

// Registered before UseAuthentication/UseAuthorization so /swagger/* is served
// by Swashbuckle's own middleware branch without ever reaching the global
// FallbackPolicy (RequireAuthenticatedUser) — otherwise every Swagger request
// gets rejected before the pipeline reaches it.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

await app.ApplyPendingMigrationsAsync();
await app.SeedInitialAdminUserAsync();

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapProductEndpoints();
app.MapLocationEndpoints();
app.MapStockMovementEndpoints();

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program
{
}

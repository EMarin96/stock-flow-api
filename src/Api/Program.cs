using StockFlow.Api.Endpoints;
using StockFlow.Api.Middleware;
using StockFlow.Application;
using StockFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();
builder.Services.AddProblemDetails();

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
}

app.MapProductEndpoints();

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

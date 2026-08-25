namespace StockFlow.Api.Configuration;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
            {
                Title = "StockFlow API",
                Version = "v1",
                Description = "Inventory management API — product catalog, stock movements, and locations.",
            });

            // Swashbuckle's schema generator does not automatically honor the
            // JsonStringEnumConverter registered in Program.cs for minimal APIs, so
            // enums (e.g. Currency) render as raw integers unless told otherwise here.
            options.SchemaFilter<EnumSchemaFilter>();

            // JWT bearer auth support in Swagger UI — lets a caller paste a token
            // obtained from POST /api/auth/login and have it sent on every
            // subsequent request the UI makes (see plan.md — Implementation).
            const string bearerSecurityScheme = "Bearer";
            options.AddSecurityDefinition(bearerSecurityScheme, new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.ParameterLocation.Header,
                Description = "Enter the JWT returned by POST /api/auth/login.",
            });
            options.AddSecurityRequirement(_ => new Microsoft.OpenApi.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.OpenApiSecuritySchemeReference(bearerSecurityScheme, null),
                    new List<string>()
                },
            });
        });

        return services;
    }
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

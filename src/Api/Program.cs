using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockFlow.Api.Endpoints;
using StockFlow.Api.HostedServices;
using StockFlow.Api.Middleware;
using StockFlow.Api.Security;
using StockFlow.Application;
using StockFlow.Application.Common.Persistence;
using StockFlow.Application.Common.Security;
using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;
using StockFlow.Infrastructure;
using StockFlow.Infrastructure.ExternalServices.CountryStateCity;
using StockFlow.Infrastructure.Persistence;
using StockFlow.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();
builder.Services.AddProblemDetails();

// Authentication/authorization (see plan.md — Implementation, step 24):
// JwtOptions is bound via the Options Pattern and consumed lazily — both by
// JwtTokenGenerator (through IOptions<JwtOptions>) and by the JwtBearer
// handler's TokenValidationParameters below, via a deferred PostConfigure
// callback rather than a value captured eagerly at startup. This matters for
// integration tests: ApiFactory overrides Jwt:Secret via
// ConfigureAppConfiguration, and that override is only visible to code that
// resolves IConfiguration/IOptions<T> through DI *after* the host finishes
// building — never to a plain local variable read synchronously from
// builder.Configuration before Build() (same reasoning already documented
// for CountryStateCityOptions, consumed lazily inside the AddHttpClient
// factory callback below).
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptionsAccessor) =>
    {
        var jwtOptions = jwtOptionsAccessor.Value;
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

// Fail-closed by default: every endpoint requires an authenticated user
// unless explicitly marked AllowAnonymous() (only POST /api/auth/login).
// WriteAccess/AdminOnly are applied per-endpoint on top of this default (see
// plan.md — Decisions).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("WriteAccess", policy => policy.RequireRole(nameof(Role.Admin), nameof(Role.Operator)));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(nameof(Role.Admin)));
});

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

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

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

// Startup admin bootstrap (unconditional — real environments apply
// migrations separately, so the Users table already exists by the time this
// runs): if the Users table is empty, seed one Admin user from
// Seed:AdminUsername/Seed:AdminPassword, or fail fast if that configuration
// is missing — otherwise the API would start with no way to log in (see
// spec.md — Bootstrap).
using (var seedScope = app.Services.CreateScope())
{
    var userWriteRepository = seedScope.ServiceProvider.GetRequiredService<IUserWriteRepository>();
    var existingUserCount = await userWriteRepository.CountAllAsync(CancellationToken.None);

    if (existingUserCount == 0)
    {
        var adminUsername = app.Configuration["Seed:AdminUsername"];
        var adminPassword = app.Configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "The Users table is empty and 'Seed:AdminUsername'/'Seed:AdminPassword' are not configured. " +
                "Set them so an initial Admin user can be created; otherwise there is no way to log in.");
        }

        var passwordHasher = seedScope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var passwordHash = passwordHasher.Hash(adminPassword);

        // createdBy: null — no authenticated actor exists yet at this point.
        var adminUser = User.Create(adminUsername, passwordHash, Role.Admin, createdBy: null);
        await userWriteRepository.AddAsync(adminUser, CancellationToken.None);

        var unitOfWork = seedScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }
}

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

using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Configuration;
using StockFlow.Api.Endpoints;
using StockFlow.Api.Middleware;
using StockFlow.Api.Security;
using StockFlow.Application;
using StockFlow.Application.Common.Persistence;
using StockFlow.Application.Common.Security;
using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;
using StockFlow.Infrastructure;
using StockFlow.Infrastructure.Persistence;

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

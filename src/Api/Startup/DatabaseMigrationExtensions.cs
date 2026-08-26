using Microsoft.EntityFrameworkCore;
using StockFlow.Infrastructure.Persistence;

namespace StockFlow.Api.Startup;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Local dev convenience only — applies any pending EF Core migrations on
    /// startup so `dotnet run` works without a separate `dotnet ef database
    /// update` step. Never runs outside Development; real environments apply
    /// migrations as an explicit, controlled deploy step.
    /// </summary>
    public static async Task ApplyPendingMigrationsAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        using var migrationScope = app.Services.CreateScope();
        var dbContext = migrationScope.ServiceProvider.GetRequiredService<StockFlowDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

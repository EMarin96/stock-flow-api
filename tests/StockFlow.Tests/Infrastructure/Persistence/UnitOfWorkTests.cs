using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Domain.Locations;
using StockFlow.Domain.Products;
using StockFlow.Domain.StockMovements;
using StockFlow.Infrastructure.Persistence;
using StockFlow.Tests.Api;

namespace StockFlow.Tests.Infrastructure.Persistence;

/// <summary>
/// Exercises UnitOfWork's DB-error translation directly against the shared
/// Testcontainers Postgres instance (no WebApplicationFactory needed — this is
/// an Infrastructure-layer concern). Deterministically proves the CHECK
/// (Quantity >= 0) constraint on StockLevels is enforced and translated into
/// DbUpdateCheckConstraintException, rather than relying on a genuinely
/// concurrent HTTP race (whose outcome/timing would be non-deterministic) —
/// see plan.md — Decisions ("Negative-stock guard enforced twice").
/// </summary>
[Collection("Postgres collection")]
public sealed class UnitOfWorkTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgresFixture;
    private StockFlowDbContext _dbContext = null!;

    public UnitOfWorkTests(PostgresContainerFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<StockFlowDbContext>()
            .UseNpgsql(_postgresFixture.ConnectionString)
            .Options;
        _dbContext = new StockFlowDbContext(options);

        await _dbContext.Database.MigrateAsync();
        await _dbContext.Database.ExecuteSqlRawAsync(
            """TRUNCATE TABLE "StockMovements", "StockLevels", "Locations", "Products";""");
    }

    public Task DisposeAsync() => _dbContext.DisposeAsync().AsTask();

    [Fact]
    public async Task SaveChangesAsync_WhenUniqueConstraintIsViolated_ThrowsDbUpdateUniqueConstraintException()
    {
        var product = Product.Create("SKU-DUP", "Widget", null, "unit", 10m, Currency.USD, 0);
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        var duplicateSkuProduct = Product.Create("SKU-DUP", "Another widget", null, "unit", 5m, Currency.USD, 0);
        await _dbContext.Products.AddAsync(duplicateSkuProduct);

        var unitOfWork = new UnitOfWork(_dbContext);

        await Assert.ThrowsAsync<DbUpdateUniqueConstraintException>(() => unitOfWork.SaveChangesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task SaveChangesAsync_WhenStockLevelQuantityWouldGoNegative_ThrowsDbUpdateCheckConstraintException()
    {
        var product = Product.Create("SKU-CHECK", "Widget", null, "unit", 10m, Currency.USD, 0);
        var location = Location.Create("WH-CHECK", "Main Warehouse", null, null, null, "CA", "Los Angeles", Country.US);
        await _dbContext.Products.AddAsync(product);
        await _dbContext.Locations.AddAsync(location);
        await _dbContext.SaveChangesAsync();

        var stockLevel = StockLevel.Create(product.Id, location.Id);
        await _dbContext.StockLevels.AddAsync(stockLevel);
        await _dbContext.SaveChangesAsync();

        // Bypasses the Domain guard clause entirely by setting the tracked
        // entity's private-setter Quantity directly — simulating the only way a
        // negative value could ever reach the database: a race the in-memory
        // guard misses (see plan.md — Decisions).
        _dbContext.Entry(stockLevel).Property(level => level.Quantity).CurrentValue = -1;

        var unitOfWork = new UnitOfWork(_dbContext);

        await Assert.ThrowsAsync<DbUpdateCheckConstraintException>(() => unitOfWork.SaveChangesAsync(CancellationToken.None));
    }
}

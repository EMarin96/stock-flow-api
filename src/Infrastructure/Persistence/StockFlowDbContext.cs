using StockFlow.Domain.Locations;
using StockFlow.Domain.Products;

namespace StockFlow.Infrastructure.Persistence;

public sealed class StockFlowDbContext(DbContextOptions<StockFlowDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Location> Locations => Set<Location>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

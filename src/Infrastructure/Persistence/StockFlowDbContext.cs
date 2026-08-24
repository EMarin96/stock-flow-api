using StockFlow.Domain.Locations;
using StockFlow.Domain.Products;
using StockFlow.Domain.StockMovements;
using StockFlow.Domain.Users;

namespace StockFlow.Infrastructure.Persistence;

public sealed class StockFlowDbContext(DbContextOptions<StockFlowDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<StockLevel> StockLevels => Set<StockLevel>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

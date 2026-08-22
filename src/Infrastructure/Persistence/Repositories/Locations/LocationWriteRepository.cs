using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;
using StockFlow.Infrastructure.Persistence;

namespace StockFlow.Infrastructure.Persistence.Repositories.Locations;

public sealed class LocationWriteRepository(StockFlowDbContext dbContext) : ILocationWriteRepository
{
    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
        dbContext.Locations.AnyAsync(location => location.Code == LocationCode.Create(code), cancellationToken);

    public Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Locations.FirstOrDefaultAsync(location => location.Id == id, cancellationToken);

    public async Task AddAsync(Location location, CancellationToken cancellationToken) =>
        await dbContext.Locations.AddAsync(location, cancellationToken);
}

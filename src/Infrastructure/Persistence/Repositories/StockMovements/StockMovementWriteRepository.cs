using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;
using StockFlow.Infrastructure.Persistence;

namespace StockFlow.Infrastructure.Persistence.Repositories.StockMovements;

public sealed class StockMovementWriteRepository(StockFlowDbContext dbContext) : IStockMovementWriteRepository
{
    public async Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken) =>
        await dbContext.StockMovements.AddAsync(stockMovement, cancellationToken);
}

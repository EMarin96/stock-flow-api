using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Infrastructure.Persistence.Repositories;

public sealed class StockMovementWriteRepository(StockFlowDbContext dbContext) : IStockMovementWriteRepository
{
    public async Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken) =>
        await dbContext.StockMovements.AddAsync(stockMovement, cancellationToken);
}

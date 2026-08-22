using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Application.StockMovements.CreateStockMovement;

public sealed record CreateStockMovementCommand(
    Guid ProductId,
    MovementType Type,
    int Quantity,
    Guid? SourceLocationId,
    Guid? DestinationLocationId,
    MovementDirection? Direction) : IRequest<Result<StockMovementDto>>;

using StockFlow.Domain.StockMovements;

namespace StockFlow.Application.StockMovements.Shared;

public sealed record StockMovementDto(
    Guid Id,
    Guid ProductId,
    MovementType Type,
    int Quantity,
    Guid? SourceLocationId,
    Guid? DestinationLocationId,
    MovementDirection? Direction,
    DateTime CreatedAt,
    Guid? CreatedBy);

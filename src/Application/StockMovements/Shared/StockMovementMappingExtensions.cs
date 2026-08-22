using StockFlow.Domain.StockMovements;

namespace StockFlow.Application.StockMovements.Shared;

public static class StockMovementMappingExtensions
{
    public static StockMovementDto ToDto(this StockMovement movement) => new(
        movement.Id,
        movement.ProductId,
        movement.Type,
        movement.Quantity,
        movement.SourceLocationId,
        movement.DestinationLocationId,
        movement.Direction,
        movement.CreatedAt,
        movement.CreatedBy);
}

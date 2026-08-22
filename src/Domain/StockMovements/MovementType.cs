namespace StockFlow.Domain.StockMovements;

/// <summary>
/// The fixed set of stock movement types (see constitution/tech-stack.md —
/// Data/domain model). Determines which location fields a
/// <see cref="StockMovement"/> requires (see <see cref="StockMovement.Create"/>).
/// </summary>
public enum MovementType
{
    In,
    Out,
    Transfer,
    Adjustment,
}

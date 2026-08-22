namespace StockFlow.Domain.StockMovements;

/// <summary>
/// Whether an <see cref="MovementType.Adjustment"/> movement increases or
/// decreases stock at the corrected location. Meaningless for any other
/// <see cref="MovementType"/> (see <see cref="StockMovement.Create"/>).
/// </summary>
public enum MovementDirection
{
    Increase,
    Decrease,
}

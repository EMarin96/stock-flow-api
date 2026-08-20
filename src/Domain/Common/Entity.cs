namespace StockFlow.Domain.Common;

/// <summary>
/// Base class for all domain entities. Holds the entity's unique identifier.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

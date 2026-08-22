namespace StockFlow.Domain.StockMovements;

/// <summary>
/// The materialized, current stock balance for one product+location pair.
/// Not computed on read — mutated atomically as a side effect of processing a
/// <see cref="StockMovement"/>, in the same transaction as the movement insert
/// (see plan.md — Approach). A client never sets <see cref="Quantity"/>
/// directly; it only ever changes via <see cref="Increase"/>/<see cref="Decrease"/>.
/// A separate entity from <see cref="StockMovement"/>, not part of the same
/// aggregate — see plan.md ("Decisions") for why this is the ledger + running
/// balance pattern rather than strict DDD aggregate composition.
/// </summary>
public class StockLevel : Entity
{
    public Guid ProductId { get; private set; }

    public Guid LocationId { get; private set; }

    public int Quantity { get; private set; }

    /// <summary>
    /// Reserved for EF Core materialization.
    /// </summary>
    private StockLevel()
    {
    }

    private StockLevel(Guid productId, Guid locationId)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        LocationId = locationId;
        Quantity = 0;
    }

    public static StockLevel Create(Guid productId, Guid locationId) => new(productId, locationId);

    public void Increase(int amount)
    {
        if (amount <= 0)
        {
            throw new DomainValidationException("Amount to increase must be greater than zero.");
        }

        Quantity += amount;
    }

    /// <summary>
    /// Guard clause throws if the decrease would leave <see cref="Quantity"/>
    /// negative. Defense-in-depth: unreachable in normal flow, since the
    /// Application handler already checks this before calling <see cref="Decrease"/>,
    /// and the DB `CHECK (Quantity >= 0)` constraint on `StockLevels` is the
    /// final backstop against races (see plan.md — Decisions).
    /// </summary>
    public void Decrease(int amount)
    {
        if (amount <= 0)
        {
            throw new DomainValidationException("Amount to decrease must be greater than zero.");
        }

        if (Quantity - amount < 0)
        {
            throw new DomainValidationException("Stock cannot go negative.");
        }

        Quantity -= amount;
    }
}

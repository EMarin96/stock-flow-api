namespace StockFlow.Domain.StockMovements;

/// <summary>
/// An immutable, append-only record of a stock change for a product. References
/// <c>Product</c> and <c>Location</c> by id only (no navigation properties) —
/// see plan.md ("StockMovement references existing Product and Location
/// aggregates by ID only"). Inherits <see cref="Entity"/>, not
/// <c>AuditableEntity</c>: only <see cref="CreatedAt"/>/<see cref="CreatedBy"/>
/// are meaningful for a record that is never updated or deleted (see
/// constitution/tech-stack.md — Hard limits).
/// </summary>
public class StockMovement : Entity
{
    public Guid ProductId { get; private set; }

    public MovementType Type { get; private set; }

    public int Quantity { get; private set; }

    public Guid? SourceLocationId { get; private set; }

    public Guid? DestinationLocationId { get; private set; }

    /// <summary>
    /// Only meaningful (and required) when <see cref="Type"/> is
    /// <see cref="MovementType.Adjustment"/>; <c>null</c> for every other type.
    /// </summary>
    public MovementDirection? Direction { get; private set; }

    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Identifier of the user who recorded this movement. Nullable and left
    /// unset until the Auth feature supplies a real user identity.
    /// </summary>
    public Guid? CreatedBy { get; private set; }

    /// <summary>
    /// Reserved for EF Core materialization.
    /// </summary>
    private StockMovement()
    {
    }

    private StockMovement(
        Guid productId,
        MovementType type,
        int quantity,
        Guid? sourceLocationId,
        Guid? destinationLocationId,
        MovementDirection? direction)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        SourceLocationId = sourceLocationId;
        DestinationLocationId = destinationLocationId;
        Direction = direction;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new movement. Guard clauses enforce: a positive quantity; the
    /// location field(s) required by <paramref name="type"/> (In -> destination
    /// only, Out -> source only, Transfer -> both, Adjustment -> destination
    /// only); and that <paramref name="direction"/> is supplied if and only if
    /// <paramref name="type"/> is <see cref="MovementType.Adjustment"/>. These
    /// duplicate the Application-layer FluentValidation checks as
    /// defense-in-depth (see <see cref="DomainValidationException"/>).
    /// </summary>
    public static StockMovement Create(
        Guid productId,
        MovementType type,
        int quantity,
        Guid? sourceLocationId,
        Guid? destinationLocationId,
        MovementDirection? direction)
    {
        if (quantity <= 0)
        {
            throw new DomainValidationException("Quantity must be greater than zero.");
        }

        switch (type)
        {
            case MovementType.In:
                if (sourceLocationId is not null)
                {
                    throw new DomainValidationException("An 'In' movement cannot have a source location.");
                }

                if (destinationLocationId is null)
                {
                    throw new DomainValidationException("An 'In' movement requires a destination location.");
                }

                break;

            case MovementType.Out:
                if (destinationLocationId is not null)
                {
                    throw new DomainValidationException("An 'Out' movement cannot have a destination location.");
                }

                if (sourceLocationId is null)
                {
                    throw new DomainValidationException("An 'Out' movement requires a source location.");
                }

                break;

            case MovementType.Transfer:
                if (sourceLocationId is null || destinationLocationId is null)
                {
                    throw new DomainValidationException("A 'Transfer' movement requires both a source and a destination location.");
                }

                break;

            case MovementType.Adjustment:
                if (sourceLocationId is not null)
                {
                    throw new DomainValidationException("An 'Adjustment' movement cannot have a source location.");
                }

                if (destinationLocationId is null)
                {
                    throw new DomainValidationException("An 'Adjustment' movement requires a destination location (the location being corrected).");
                }

                break;

            default:
                throw new DomainValidationException("Type is not a valid movement type.");
        }

        if (type == MovementType.Adjustment && direction is null)
        {
            throw new DomainValidationException("Direction is required for an 'Adjustment' movement.");
        }

        if (type != MovementType.Adjustment && direction is not null)
        {
            throw new DomainValidationException("Direction is only allowed for an 'Adjustment' movement.");
        }

        return new StockMovement(productId, type, quantity, sourceLocationId, destinationLocationId, direction);
    }
}

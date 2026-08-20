namespace StockFlow.Domain.Common;

/// <summary>
/// Base class for entities that require audit metadata (who/when created and
/// last updated) and soft-delete support. No domain entity is ever physically
/// removed from the database — see the "soft delete" convention in tech-stack.md.
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who created this record. Nullable and left unset
    /// until the Auth feature supplies a real user identity.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who last updated this record. Nullable and left
    /// unset until the Auth feature supplies a real user identity.
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}

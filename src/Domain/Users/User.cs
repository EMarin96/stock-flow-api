namespace StockFlow.Domain.Users;

/// <summary>
/// An authenticated user of the API. Exactly one fixed <see cref="Role"/>
/// determines the user's permissions everywhere else (see
/// constitution/tech-stack.md — Data/domain model). Deactivation reuses the
/// inherited <see cref="AuditableEntity.IsDeleted"/> soft-delete flag, same
/// convention as <c>Product</c>/<c>Location</c>.
/// </summary>
public class User : AuditableEntity
{
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// <see cref="Username"/> upper-invariant, used to enforce case-insensitive
    /// uniqueness without a Postgres-specific <c>citext</c> column — mirrors
    /// ASP.NET Core Identity's own normalized-username convention (see plan.md
    /// — Decisions).
    /// </summary>
    public string NormalizedUsername { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public Role Role { get; private set; }

    /// <summary>
    /// Reserved for EF Core materialization.
    /// </summary>
    private User()
    {
    }

    private User(string username, string passwordHash, Role role, Guid? createdBy)
    {
        Id = Guid.NewGuid();
        Username = username;
        NormalizedUsername = username.ToUpperInvariant();
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Creates a new user. <paramref name="passwordHash"/> must already be
    /// hashed — hashing is an Infrastructure concern (<c>IPasswordHasher</c>),
    /// not a Domain one (see plan.md — Implementation).
    /// </summary>
    public static User Create(string username, string passwordHash, Role role, Guid? createdBy)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new DomainValidationException("Username cannot be empty.");
        }

        if (username.Length > 100)
        {
            throw new DomainValidationException("Username cannot be longer than 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException("Password hash cannot be empty.");
        }

        return new User(username, passwordHash, role, createdBy);
    }

    public void UpdateRole(Role role, Guid? updatedBy)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void ResetPassword(string passwordHash, Guid? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException("Password hash cannot be empty.");
        }

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}

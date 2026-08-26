using StockFlow.Domain.Users;

namespace StockFlow.Application.Users.Shared;

/// <summary>
/// Write-side user access, backed by EF Core. Used by command handlers.
/// </summary>
public interface IUserWriteRepository
{
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a user by id. Soft-deleted (deactivated) users are excluded
    /// automatically by EF Core's global query filter.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// A raw count bypassing the soft-delete filter, used only by the startup
    /// admin-bootstrap block to decide whether the <c>Users</c> table is empty
    /// (see plan.md — Implementation, step 24).
    /// </summary>
    Task<int> CountAllAsync(CancellationToken cancellationToken);
}

using StockFlow.Domain.Users;

namespace StockFlow.Application.Users.Shared;

/// <summary>
/// Read-side user access, backed by Dapper. Used by query handlers and the
/// login flow. Implementations must explicitly filter out soft-deleted
/// (deactivated) users (IsDeleted = false) since Dapper does not participate
/// in EF Core's global query filters — except <see cref="GetForAuthenticationAsync"/>,
/// which deliberately returns deactivated users too so the login handler can
/// distinguish and still return the same generic error (see plan.md).
/// </summary>
public interface IUserReadRepository
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<UserDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? usernameFilter,
        Role? role,
        CancellationToken cancellationToken);

    /// <summary>
    /// Looks up a user (including deactivated ones) by username for the login
    /// flow. Never used outside login — see <see cref="AuthenticationRecord"/>.
    /// </summary>
    Task<AuthenticationRecord?> GetForAuthenticationAsync(string username, CancellationToken cancellationToken);
}

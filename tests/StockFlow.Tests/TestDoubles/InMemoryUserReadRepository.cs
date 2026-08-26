using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="IUserReadRepository"/> used by handler
/// unit tests. Mirrors the Dapper implementation's soft-delete filtering
/// (except <see cref="GetForAuthenticationAsync"/>, same as the real one).
/// </summary>
public sealed class InMemoryUserReadRepository(IEnumerable<AuthenticationRecord> seed) : IUserReadRepository
{
    private readonly List<AuthenticationRecord> _users = seed.ToList();

    public Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = _users.FirstOrDefault(user => user.Id == id && !user.IsDeleted);
        return Task.FromResult(user is null ? null : ToDto(user));
    }

    public Task<PagedResult<UserDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? usernameFilter,
        Role? role,
        CancellationToken cancellationToken)
    {
        var filtered = _users
            .Where(user => !user.IsDeleted)
            .Where(user => usernameFilter is null || user.Username.Contains(usernameFilter, StringComparison.OrdinalIgnoreCase))
            .Where(user => role is null || user.Role == role)
            .ToList();

        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDto)
            .ToList();

        return Task.FromResult(new PagedResult<UserDto>(items, page, pageSize, filtered.Count));
    }

    public Task<AuthenticationRecord?> GetForAuthenticationAsync(string username, CancellationToken cancellationToken)
    {
        var normalizedUsername = username.ToUpperInvariant();
        var user = _users.FirstOrDefault(user => user.Username.ToUpperInvariant() == normalizedUsername);
        return Task.FromResult(user);
    }

    private static UserDto ToDto(AuthenticationRecord record) =>
        new(record.Id, record.Username, record.Role, DateTime.UtcNow, null, null, null);
}

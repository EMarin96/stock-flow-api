using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="IUserWriteRepository"/> used by handler
/// unit tests, so tests don't depend on EF Core or a live database.
/// </summary>
public sealed class InMemoryUserWriteRepository : IUserWriteRepository
{
    private readonly List<User> _users;

    public InMemoryUserWriteRepository(IEnumerable<User>? seed = null)
    {
        _users = seed?.ToList() ?? [];
    }

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken)
    {
        var normalizedUsername = username.ToUpperInvariant();
        return Task.FromResult(_users.Any(user => !user.IsDeleted && user.NormalizedUsername == normalizedUsername));
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_users.FirstOrDefault(user => user.Id == id && !user.IsDeleted));

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task<int> CountAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_users.Count);
}

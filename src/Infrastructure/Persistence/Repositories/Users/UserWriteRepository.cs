using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;
using StockFlow.Infrastructure.Persistence;

namespace StockFlow.Infrastructure.Persistence.Repositories.Users;

public sealed class UserWriteRepository(StockFlowDbContext dbContext) : IUserWriteRepository
{
    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken)
    {
        var normalizedUsername = username.ToUpperInvariant();
        return dbContext.Users.AnyAsync(user => user.NormalizedUsername == normalizedUsername, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken) =>
        await dbContext.Users.AddAsync(user, cancellationToken);

    public Task<int> CountAllAsync(CancellationToken cancellationToken) =>
        dbContext.Users.IgnoreQueryFilters().CountAsync(cancellationToken);
}

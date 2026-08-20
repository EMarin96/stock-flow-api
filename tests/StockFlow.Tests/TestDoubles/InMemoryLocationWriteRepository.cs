using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="ILocationWriteRepository"/> used by handler
/// unit tests, so tests don't depend on EF Core or a live database.
/// </summary>
public sealed class InMemoryLocationWriteRepository : ILocationWriteRepository
{
    private readonly List<Location> _locations;

    public InMemoryLocationWriteRepository(IEnumerable<Location>? seed = null)
    {
        _locations = seed?.ToList() ?? [];
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
        Task.FromResult(_locations.Any(location => !location.IsDeleted && location.Code.Value == code));

    public Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_locations.FirstOrDefault(location => location.Id == id && !location.IsDeleted));

    public Task AddAsync(Location location, CancellationToken cancellationToken)
    {
        _locations.Add(location);
        return Task.CompletedTask;
    }
}

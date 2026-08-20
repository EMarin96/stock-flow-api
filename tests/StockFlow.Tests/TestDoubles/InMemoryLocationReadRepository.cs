using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Locations.Shared;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="ILocationReadRepository"/> used by handler
/// unit tests. Mirrors the Dapper implementation's soft-delete filtering.
/// </summary>
public sealed class InMemoryLocationReadRepository(IEnumerable<LocationDto> seed) : ILocationReadRepository
{
    private readonly List<LocationDto> _locations = seed.ToList();

    public Task<LocationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_locations.FirstOrDefault(location => location.Id == id));

    public Task<PagedResult<LocationDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var items = _locations
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<LocationDto>(items, page, pageSize, _locations.Count));
    }
}

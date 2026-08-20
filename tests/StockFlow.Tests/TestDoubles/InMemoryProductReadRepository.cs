using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Products.Shared;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="IProductReadRepository"/> used by handler
/// unit tests. Mirrors the Dapper implementation's soft-delete filtering.
/// </summary>
public sealed class InMemoryProductReadRepository(IEnumerable<ProductDto> seed) : IProductReadRepository
{
    private readonly List<ProductDto> _products = seed.ToList();

    public Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_products.FirstOrDefault(product => product.Id == id));

    public Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var items = _products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<ProductDto>(items, page, pageSize, _products.Count));
    }
}

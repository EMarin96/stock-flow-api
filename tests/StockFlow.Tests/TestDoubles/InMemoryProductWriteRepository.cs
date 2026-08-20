using StockFlow.Application.Products.Shared;
using StockFlow.Domain.Products;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="IProductWriteRepository"/> used by handler
/// unit tests, so tests don't depend on EF Core or a live database.
/// </summary>
public sealed class InMemoryProductWriteRepository : IProductWriteRepository
{
    private readonly List<Product> _products;

    public InMemoryProductWriteRepository(IEnumerable<Product>? seed = null)
    {
        _products = seed?.ToList() ?? [];
    }

    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken) =>
        Task.FromResult(_products.Any(product => !product.IsDeleted && product.Sku.Value == sku));

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_products.FirstOrDefault(product => product.Id == id && !product.IsDeleted));

    public Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        _products.Add(product);
        return Task.CompletedTask;
    }
}

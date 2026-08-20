using StockFlow.Domain.Products;

namespace StockFlow.Application.Products.Shared;

/// <summary>
/// Write-side product access, backed by EF Core. Used by command handlers.
/// </summary>
public interface IProductWriteRepository
{
    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a product by id. Soft-deleted products are excluded automatically
    /// by EF Core's global query filter.
    /// </summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);
}

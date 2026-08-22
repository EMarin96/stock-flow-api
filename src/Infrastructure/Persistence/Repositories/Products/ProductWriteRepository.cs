using StockFlow.Application.Products.Shared;
using StockFlow.Domain.Products;
using StockFlow.Infrastructure.Persistence;

namespace StockFlow.Infrastructure.Persistence.Repositories.Products;

public sealed class ProductWriteRepository(StockFlowDbContext dbContext) : IProductWriteRepository
{
    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(product => product.Sku == Sku.Create(sku), cancellationToken);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken) =>
        await dbContext.Products.AddAsync(product, cancellationToken);
}

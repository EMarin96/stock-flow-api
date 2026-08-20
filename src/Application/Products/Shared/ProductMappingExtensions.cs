using StockFlow.Domain.Products;

namespace StockFlow.Application.Products.Shared;

public static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Product product) => new(
        product.Id,
        product.Sku.Value,
        product.Name,
        product.Description,
        product.UnitOfMeasure,
        product.Price.Amount,
        product.Price.Currency,
        product.MinimumStockThreshold,
        product.CreatedAt,
        product.CreatedBy,
        product.UpdatedAt,
        product.UpdatedBy);
}

using StockFlow.Application.Products.Shared;
using StockFlow.Domain.Products;

namespace StockFlow.Application.Products.UpdateProduct;

// The SKU is intentionally not part of this command — it is immutable once created.
public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    string UnitOfMeasure,
    decimal Price,
    Currency Currency,
    int MinimumStockThreshold) : IRequest<Result<ProductDto>>;

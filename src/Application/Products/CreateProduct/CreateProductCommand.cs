using StockFlow.Application.Products.Shared;
using StockFlow.Domain.Products;

namespace StockFlow.Application.Products.CreateProduct;

public sealed record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    string UnitOfMeasure,
    decimal Price,
    Currency Currency,
    int MinimumStockThreshold) : IRequest<Result<ProductDto>>;

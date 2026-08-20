using StockFlow.Domain.Products;

namespace StockFlow.Application.Products.Shared;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string UnitOfMeasure,
    decimal Price,
    Currency Currency,
    int MinimumStockThreshold,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy);

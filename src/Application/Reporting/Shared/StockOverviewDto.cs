namespace StockFlow.Application.Reporting.Shared;

public sealed record StockOverviewDto(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    int MinimumStockThreshold,
    int TotalQuantity,
    bool IsLowStock);

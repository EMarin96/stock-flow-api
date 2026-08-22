namespace StockFlow.Application.StockMovements.Shared;

/// <summary>
/// One row of GetLocationStock's response: a product and its current stock at
/// the queried location (see plan.md — Implementation, step 8).
/// </summary>
public sealed record LocationStockDto(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    int Quantity);

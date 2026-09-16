using StockFlow.Domain.StockMovements;

namespace StockFlow.Application.Reporting.Shared;

public sealed record MovementActivityDto(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    MovementType Type,
    int MovementCount,
    int TotalQuantity);

using StockFlow.Domain.StockMovements;

namespace StockFlow.Application.StockMovements.Shared;

/// <summary>
/// Write-side stock movement access, backed by EF Core. Used by
/// CreateStockMovementHandler. Append-only — there is deliberately no
/// GetByIdAsync/Update here, since movements are never edited or deleted
/// (see constitution/tech-stack.md — Hard limits).
/// </summary>
public interface IStockMovementWriteRepository
{
    Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken);
}

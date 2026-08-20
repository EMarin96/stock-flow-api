using StockFlow.Domain.Locations;

namespace StockFlow.Application.Locations.Shared;

/// <summary>
/// Write-side location access, backed by EF Core. Used by command handlers.
/// </summary>
public interface ILocationWriteRepository
{
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a location by id. Soft-deleted locations are excluded automatically
    /// by EF Core's global query filter.
    /// </summary>
    Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Location location, CancellationToken cancellationToken);
}

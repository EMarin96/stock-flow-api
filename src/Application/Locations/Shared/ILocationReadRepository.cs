namespace StockFlow.Application.Locations.Shared;

/// <summary>
/// Read-side location access, backed by Dapper. Used by query handlers.
/// Implementations must explicitly filter out soft-deleted locations
/// (IsDeleted = false) since Dapper does not participate in EF Core's
/// global query filters.
/// </summary>
public interface ILocationReadRepository
{
    Task<LocationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<LocationDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
}

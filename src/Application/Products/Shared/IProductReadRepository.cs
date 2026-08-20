namespace StockFlow.Application.Products.Shared;

/// <summary>
/// Read-side product access, backed by Dapper. Used by query handlers.
/// Implementations must explicitly filter out soft-deleted products
/// (IsDeleted = false) since Dapper does not participate in EF Core's
/// global query filters.
/// </summary>
public interface IProductReadRepository
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
}

using Dapper;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.StockMovements.Shared;
using StockFlow.Infrastructure.Persistence.Read;

namespace StockFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Dapper-backed read repository for location stock. LEFT JOINs StockLevels so
/// every active product for the queried location is returned, including ones
/// with no StockLevel row yet (surfaced as Quantity = 0, via COALESCE) — see
/// plan.md — Implementation, step 8.
/// </summary>
public sealed class StockLevelReadRepository(ISqlConnectionFactory connectionFactory) : IStockLevelReadRepository
{
    public async Task<PagedResult<LocationStockDto>> GetPagedByLocationAsync(
        Guid locationId,
        string? productNameFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                p."Id" AS "ProductId", p."Sku" AS "ProductSku", p."Name" AS "ProductName",
                COALESCE(sl."Quantity", 0) AS "Quantity"
            FROM "Products" p
            LEFT JOIN "StockLevels" sl ON sl."ProductId" = p."Id" AND sl."LocationId" = @LocationId
            WHERE p."IsDeleted" = false
              AND (@ProductNameFilter::text IS NULL OR p."Name" ILIKE '%' || @ProductNameFilter || '%')
            ORDER BY p."Name"
            OFFSET @Offset LIMIT @PageSize;

            SELECT COUNT(*)
            FROM "Products" p
            WHERE p."IsDeleted" = false
              AND (@ProductNameFilter::text IS NULL OR p."Name" ILIKE '%' || @ProductNameFilter || '%');
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new
            {
                LocationId = locationId,
                ProductNameFilter = productNameFilter,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize,
            },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<LocationStockDto>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<LocationStockDto>(items, page, pageSize, totalCount);
    }
}

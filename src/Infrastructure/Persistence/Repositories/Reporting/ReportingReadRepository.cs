using Dapper;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Reporting.Shared;

namespace StockFlow.Infrastructure.Persistence.Repositories.Reporting;

/// <summary>
/// Dapper-backed read repository for both reporting endpoints (see plan.md —
/// Decisions, "a single IReportingReadRepository with two methods").
/// </summary>
public sealed class ReportingReadRepository(ISqlConnectionFactory connectionFactory) : IReportingReadRepository
{
    public async Task<PagedResult<StockOverviewDto>> GetStockOverviewAsync(
        int page,
        int pageSize,
        bool? lowStockOnly,
        CancellationToken cancellationToken)
    {
        const string sql = """
            WITH product_totals AS (
                SELECT
                    p."Id" AS "ProductId", p."Sku" AS "ProductSku", p."Name" AS "ProductName",
                    p."MinimumStockThreshold",
                    COALESCE(SUM(sl."Quantity"), 0)::int AS "TotalQuantity"
                FROM "Products" p
                LEFT JOIN "StockLevels" sl ON sl."ProductId" = p."Id"
                WHERE p."IsDeleted" = false
                GROUP BY p."Id", p."Sku", p."Name", p."MinimumStockThreshold"
            )
            SELECT
                "ProductId", "ProductSku", "ProductName", "MinimumStockThreshold", "TotalQuantity",
                ("TotalQuantity" <= "MinimumStockThreshold") AS "IsLowStock"
            FROM product_totals
            WHERE (@LowStockOnly IS NOT TRUE OR "TotalQuantity" <= "MinimumStockThreshold")
            ORDER BY "ProductName"
            OFFSET @Offset LIMIT @PageSize;

            WITH product_totals AS (
                SELECT
                    p."Id" AS "ProductId", p."MinimumStockThreshold",
                    COALESCE(SUM(sl."Quantity"), 0)::int AS "TotalQuantity"
                FROM "Products" p
                LEFT JOIN "StockLevels" sl ON sl."ProductId" = p."Id"
                WHERE p."IsDeleted" = false
                GROUP BY p."Id", p."MinimumStockThreshold"
            )
            SELECT COUNT(*)
            FROM product_totals
            WHERE (@LowStockOnly IS NOT TRUE OR "TotalQuantity" <= "MinimumStockThreshold");
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new
            {
                LowStockOnly = lowStockOnly,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize,
            },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<StockOverviewDto>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<StockOverviewDto>(items, page, pageSize, totalCount);
    }

    public async Task<PagedResult<MovementActivityDto>> GetMovementActivityAsync(
        DateTime? from,
        DateTime? to,
        Guid? productId,
        Guid? locationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                p."Id" AS "ProductId", p."Sku" AS "ProductSku", p."Name" AS "ProductName", sm."Type",
                COUNT(*)::int AS "MovementCount", SUM(sm."Quantity")::int AS "TotalQuantity"
            FROM "StockMovements" sm
            JOIN "Products" p ON p."Id" = sm."ProductId"
            WHERE (@From::timestamp IS NULL OR sm."CreatedAt" >= @From)
              AND (@To::timestamp IS NULL OR sm."CreatedAt" <= @To)
              AND (@ProductId::uuid IS NULL OR sm."ProductId" = @ProductId)
              AND (@LocationId::uuid IS NULL OR sm."SourceLocationId" = @LocationId OR sm."DestinationLocationId" = @LocationId)
            GROUP BY p."Id", p."Sku", p."Name", sm."Type"
            ORDER BY p."Name", sm."Type"
            OFFSET @Offset LIMIT @PageSize;

            SELECT COUNT(*)
            FROM (
                SELECT 1
                FROM "StockMovements" sm
                JOIN "Products" p ON p."Id" = sm."ProductId"
                WHERE (@From::timestamp IS NULL OR sm."CreatedAt" >= @From)
                  AND (@To::timestamp IS NULL OR sm."CreatedAt" <= @To)
                  AND (@ProductId::uuid IS NULL OR sm."ProductId" = @ProductId)
                  AND (@LocationId::uuid IS NULL OR sm."SourceLocationId" = @LocationId OR sm."DestinationLocationId" = @LocationId)
                GROUP BY p."Id", sm."Type"
            ) grouped;
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new
            {
                From = from,
                To = to,
                ProductId = productId,
                LocationId = locationId,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize,
            },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<MovementActivityDto>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<MovementActivityDto>(items, page, pageSize, totalCount);
    }
}

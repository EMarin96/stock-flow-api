using Dapper;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;
using StockFlow.Infrastructure.Persistence.Read;

namespace StockFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Dapper-backed read repository for stock movements. StockMovements has no
/// soft-delete flag (movements are append-only, see
/// constitution/tech-stack.md — Hard limits), so no IsDeleted filter is needed.
/// </summary>
public sealed class StockMovementReadRepository(ISqlConnectionFactory connectionFactory) : IStockMovementReadRepository
{
    public async Task<PagedResult<StockMovementDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? productId,
        Guid? locationId,
        MovementType? type,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                "Id", "ProductId", "Type", "Quantity", "SourceLocationId", "DestinationLocationId",
                "Direction", "CreatedAt", "CreatedBy"
            FROM "StockMovements"
            WHERE (@ProductId::uuid IS NULL OR "ProductId" = @ProductId)
              AND (@LocationId::uuid IS NULL OR "SourceLocationId" = @LocationId OR "DestinationLocationId" = @LocationId)
              AND (@Type::text IS NULL OR "Type" = @Type)
            ORDER BY "CreatedAt" DESC
            OFFSET @Offset LIMIT @PageSize;

            SELECT COUNT(*)
            FROM "StockMovements"
            WHERE (@ProductId::uuid IS NULL OR "ProductId" = @ProductId)
              AND (@LocationId::uuid IS NULL OR "SourceLocationId" = @LocationId OR "DestinationLocationId" = @LocationId)
              AND (@Type::text IS NULL OR "Type" = @Type);
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new
            {
                ProductId = productId,
                LocationId = locationId,
                Type = type?.ToString(),
                Offset = (page - 1) * pageSize,
                PageSize = pageSize,
            },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<StockMovementDto>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<StockMovementDto>(items, page, pageSize, totalCount);
    }
}

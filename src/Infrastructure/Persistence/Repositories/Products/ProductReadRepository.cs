using Dapper;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Products.Shared;

namespace StockFlow.Infrastructure.Persistence.Repositories.Products;

/// <summary>
/// Dapper-backed read repository for products. Every query filters
/// "IsDeleted = false" explicitly, since Dapper does not participate in EF
/// Core's global query filter (see plan.md — Risks).
/// </summary>
public sealed class ProductReadRepository(ISqlConnectionFactory connectionFactory) : IProductReadRepository
{
    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                "Id", "Sku", "Name", "Description", "UnitOfMeasure", "Price", "Currency",
                "MinimumStockThreshold", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"
            FROM "Products"
            WHERE "Id" = @Id AND "IsDeleted" = false
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ProductDto>(command);
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                "Id", "Sku", "Name", "Description", "UnitOfMeasure", "Price", "Currency",
                "MinimumStockThreshold", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"
            FROM "Products"
            WHERE "IsDeleted" = false
            ORDER BY "CreatedAt" DESC
            OFFSET @Offset LIMIT @PageSize;

            SELECT COUNT(*) FROM "Products" WHERE "IsDeleted" = false;
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new { Offset = (page - 1) * pageSize, PageSize = pageSize },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<ProductDto>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<ProductDto>(items, page, pageSize, totalCount);
    }
}

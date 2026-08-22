using Dapper;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Locations.Shared;

namespace StockFlow.Infrastructure.Persistence.Repositories.Locations;

/// <summary>
/// Dapper-backed read repository for locations. Every query filters
/// "IsDeleted = false" explicitly, since Dapper does not participate in EF
/// Core's global query filter (see plan.md — Risks).
/// </summary>
public sealed class LocationReadRepository(ISqlConnectionFactory connectionFactory) : ILocationReadRepository
{
    public async Task<LocationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                "Id", "Code", "Name", "AddressLine1", "AddressLine2", "AddressLine3",
                "State", "City", "Country",
                "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"
            FROM "Locations"
            WHERE "Id" = @Id AND "IsDeleted" = false
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<LocationDto>(command);
    }

    public async Task<PagedResult<LocationDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                "Id", "Code", "Name", "AddressLine1", "AddressLine2", "AddressLine3",
                "State", "City", "Country",
                "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"
            FROM "Locations"
            WHERE "IsDeleted" = false
            ORDER BY "CreatedAt" DESC
            OFFSET @Offset LIMIT @PageSize;

            SELECT COUNT(*) FROM "Locations" WHERE "IsDeleted" = false;
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new { Offset = (page - 1) * pageSize, PageSize = pageSize },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<LocationDto>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<LocationDto>(items, page, pageSize, totalCount);
    }
}

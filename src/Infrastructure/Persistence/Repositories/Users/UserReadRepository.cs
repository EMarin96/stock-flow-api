using Dapper;
using StockFlow.Application.Common.Pagination;
using StockFlow.Application.Users.Shared;
using StockFlow.Domain.Users;

namespace StockFlow.Infrastructure.Persistence.Repositories.Users;

/// <summary>
/// Dapper-backed read repository for users. Every query except
/// <see cref="GetForAuthenticationAsync"/> filters "IsDeleted = false"
/// explicitly, since Dapper does not participate in EF Core's global query
/// filter (see plan.md — Risks); no query except
/// <see cref="GetForAuthenticationAsync"/> ever selects "PasswordHash" (see
/// plan.md — Decisions).
/// </summary>
public sealed class UserReadRepository(ISqlConnectionFactory connectionFactory) : IUserReadRepository
{
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                "Id", "Username", "Role", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"
            FROM "Users"
            WHERE "Id" = @Id AND "IsDeleted" = false
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<UserDto>(command);
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? usernameFilter,
        Role? role,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                "Id", "Username", "Role", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"
            FROM "Users"
            WHERE "IsDeleted" = false
                AND (@UsernameFilter IS NULL OR "Username" ILIKE '%' || @UsernameFilter || '%')
                AND (@Role IS NULL OR "Role" = @Role)
            ORDER BY "CreatedAt" DESC
            OFFSET @Offset LIMIT @PageSize;

            SELECT COUNT(*)
            FROM "Users"
            WHERE "IsDeleted" = false
                AND (@UsernameFilter IS NULL OR "Username" ILIKE '%' || @UsernameFilter || '%')
                AND (@Role IS NULL OR "Role" = @Role);
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new
            {
                Offset = (page - 1) * pageSize,
                PageSize = pageSize,
                UsernameFilter = usernameFilter,
                Role = role?.ToString(),
            },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<UserDto>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<UserDto>(items, page, pageSize, totalCount);
    }

    public async Task<AuthenticationRecord?> GetForAuthenticationAsync(string username, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                "Id", "Username", "PasswordHash", "Role", "IsDeleted"
            FROM "Users"
            WHERE "NormalizedUsername" = @NormalizedUsername
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new { NormalizedUsername = username.ToUpperInvariant() },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<AuthenticationRecord>(command);
    }
}

using System.Data;

namespace StockFlow.Infrastructure.Persistence.Read;

/// <summary>
/// Creates raw ADO.NET connections for Dapper read queries, kept separate from
/// EF Core's <see cref="StockFlowDbContext"/> per the CQRS convention in
/// tech-stack.md (EF Core for writes, Dapper for reads).
/// </summary>
public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}

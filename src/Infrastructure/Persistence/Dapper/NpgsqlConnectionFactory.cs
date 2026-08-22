using System.Data;
using Npgsql;

namespace StockFlow.Infrastructure.Persistence.Dapper;

public sealed class NpgsqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection() => new NpgsqlConnection(connectionString);
}

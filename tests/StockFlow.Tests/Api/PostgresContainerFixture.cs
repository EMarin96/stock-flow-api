using Testcontainers.PostgreSql;

namespace StockFlow.Tests.Api;

/// <summary>
/// Starts a single disposable Postgres container (via Testcontainers) shared
/// across the whole test run, so integration tests never touch the local dev
/// database started by docker-compose. Requires Docker to be running locally.
/// </summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    public string ConnectionString => Container.GetConnectionString();

    public Task InitializeAsync() => Container.StartAsync();

    public Task DisposeAsync() => Container.DisposeAsync().AsTask();
}

[CollectionDefinition("Postgres collection")]
public sealed class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
{
}

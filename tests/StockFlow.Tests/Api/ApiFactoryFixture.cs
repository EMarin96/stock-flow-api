namespace StockFlow.Tests.Api;

/// <summary>
/// Builds a single <see cref="ApiFactory"/> (and its TestServer/host) per test
/// class, pointed at the connection string of the shared Testcontainers
/// Postgres instance (<see cref="PostgresContainerFixture"/>). xUnit resolves
/// collection fixtures into class fixture constructors, so this composes
/// automatically for any test class that is both [Collection("Postgres
/// collection")] and IClassFixture&lt;ApiFactoryFixture&gt;. Migrations run
/// once here, in <see cref="InitializeAsync"/>; per-test cleanup (TRUNCATE)
/// stays the test class's responsibility.
/// </summary>
public sealed class ApiFactoryFixture : IAsyncLifetime
{
    public ApiFactory Factory { get; }

    public ApiFactoryFixture(PostgresContainerFixture postgresFixture)
    {
        Factory = new ApiFactory(postgresFixture.ConnectionString);
    }

    public Task InitializeAsync() => Factory.InitializeDatabaseAsync();

    public Task DisposeAsync()
    {
        Factory.Dispose();
        return Task.CompletedTask;
    }
}

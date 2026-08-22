namespace StockFlow.Tests;

/// <summary>
/// Single source of truth for resetting the shared Testcontainers Postgres
/// database between tests. Postgres requires every FK-related table to be
/// truncated together (StockLevels/StockMovements both FK to Products and
/// Locations), so this list grows in one place, not one per consumer.
/// </summary>
public static class TestDatabase
{
    public const string TruncateAllTablesSql =
        """TRUNCATE TABLE "StockMovements", "StockLevels", "Locations", "Products";""";
}

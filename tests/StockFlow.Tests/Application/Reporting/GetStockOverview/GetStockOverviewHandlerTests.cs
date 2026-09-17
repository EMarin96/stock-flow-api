using StockFlow.Application.Reporting.GetStockOverview;
using StockFlow.Application.Reporting.Shared;

namespace StockFlow.Tests.Application.Reporting.GetStockOverview;

public class GetStockOverviewHandlerTests
{
    private static StockOverviewDto SampleDto(bool isLowStock = false) => new(
        Guid.NewGuid(), "SKU-1", "Widget", 10, isLowStock ? 5 : 50, isLowStock);

    [Fact]
    public async Task GetStockOverview_WhenCalled_ReturnsPagedOverview()
    {
        var repository = new InMemoryReportingReadRepository(stockOverviewSeed: [SampleDto(), SampleDto(), SampleDto()]);
        var handler = new GetStockOverviewHandler(repository, new GetStockOverviewValidator());

        var result = await handler.Handle(new GetStockOverviewQuery(1, 2, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(3, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetStockOverview_WithLowStockOnly_ReturnsOnlyFlaggedProducts()
    {
        var repository = new InMemoryReportingReadRepository(
            stockOverviewSeed: [SampleDto(isLowStock: true), SampleDto(isLowStock: false)]);
        var handler = new GetStockOverviewHandler(repository, new GetStockOverviewValidator());

        var result = await handler.Handle(new GetStockOverviewQuery(1, 20, true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.True(result.Value.Items[0].IsLowStock);
    }

    [Fact]
    public async Task GetStockOverview_WithLowStockOnlyFalse_ReturnsEveryProduct()
    {
        var repository = new InMemoryReportingReadRepository(
            stockOverviewSeed: [SampleDto(isLowStock: true), SampleDto(isLowStock: false)]);
        var handler = new GetStockOverviewHandler(repository, new GetStockOverviewValidator());

        var result = await handler.Handle(new GetStockOverviewQuery(1, 20, false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetStockOverview_WithInvalidPageSize_ReturnsValidationError()
    {
        var repository = new InMemoryReportingReadRepository();
        var handler = new GetStockOverviewHandler(repository, new GetStockOverviewValidator());

        var result = await handler.Handle(new GetStockOverviewQuery(1, 0, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}

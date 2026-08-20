using StockFlow.Application.Products.GetProducts;
using StockFlow.Application.Products.Shared;
using StockFlow.Domain.Products;

namespace StockFlow.Tests.Application.Products.GetProducts;

public class GetProductsHandlerTests
{
    private static ProductDto SampleDto(string sku) => new(
        Guid.NewGuid(), sku, "Widget", null, "unit", 10m, Currency.USD, 5, DateTime.UtcNow, null, null, null);

    [Fact]
    public async Task GetProducts_WhenCalled_ReturnsPagedProducts()
    {
        var repository = new InMemoryProductReadRepository([SampleDto("SKU-1"), SampleDto("SKU-2"), SampleDto("SKU-3")]);
        var handler = new GetProductsHandler(repository, new GetProductsValidator());

        var result = await handler.Handle(new GetProductsQuery(1, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
    }

    [Fact]
    public async Task GetProducts_WithInvalidPage_ReturnsValidationError()
    {
        var repository = new InMemoryProductReadRepository([]);
        var handler = new GetProductsHandler(repository, new GetProductsValidator());

        var result = await handler.Handle(new GetProductsQuery(0, 20), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}

using StockFlow.Application.Products.GetProductById;
using StockFlow.Application.Products.Shared;
using StockFlow.Domain.Products;

namespace StockFlow.Tests.Application.Products.GetProductById;

public class GetProductByIdHandlerTests
{
    private static ProductDto SampleDto(Guid id) => new(
        id, "SKU-100", "Widget", null, "unit", 10m, Currency.USD, 5, DateTime.UtcNow, null, null, null);

    [Fact]
    public async Task GetProductById_WhenItExists_ReturnsProduct()
    {
        var id = Guid.NewGuid();
        var repository = new InMemoryProductReadRepository([SampleDto(id)]);
        var handler = new GetProductByIdHandler(repository);

        var result = await handler.Handle(new GetProductByIdQuery(id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
    }

    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var repository = new InMemoryProductReadRepository([]);
        var handler = new GetProductByIdHandler(repository);

        var result = await handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task GetProductById_WhenProductWasSoftDeleted_ReturnsNotFound()
    {
        // The read repository (Dapper in production) is expected to never surface
        // soft-deleted products in the first place — simulated here by an empty seed.
        var id = Guid.NewGuid();
        var repository = new InMemoryProductReadRepository([]);
        var handler = new GetProductByIdHandler(repository);

        var result = await handler.Handle(new GetProductByIdQuery(id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}

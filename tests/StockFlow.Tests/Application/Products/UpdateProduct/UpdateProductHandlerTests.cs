using StockFlow.Application.Products.UpdateProduct;
using StockFlow.Domain.Products;

namespace StockFlow.Tests.Application.Products.UpdateProduct;

public class UpdateProductHandlerTests
{
    [Fact]
    public async Task UpdateProduct_WhenItExists_UpdatesProduct()
    {
        var product = Product.Create("SKU-100", "Widget", null, "unit", 10m, Currency.USD, 5);
        var repository = new InMemoryProductWriteRepository([product]);
        var handler = new UpdateProductHandler(repository, new InMemoryUnitOfWork(), new UpdateProductValidator());

        var command = new UpdateProductCommand(product.Id, "Widget v2", "Updated", "box", 20m, Currency.CRC, 10);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Widget v2", result.Value.Name);
        Assert.Equal("SKU-100", result.Value.Sku); // SKU stays unchanged
        Assert.Equal(Currency.CRC, result.Value.Currency);
    }

    [Fact]
    public async Task UpdateProduct_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var repository = new InMemoryProductWriteRepository();
        var handler = new UpdateProductHandler(repository, new InMemoryUnitOfWork(), new UpdateProductValidator());

        var command = new UpdateProductCommand(Guid.NewGuid(), "Widget", null, "unit", 10m, Currency.USD, 5);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task UpdateProduct_WhenProductIsSoftDeleted_ReturnsNotFound()
    {
        var product = Product.Create("SKU-100", "Widget", null, "unit", 10m, Currency.USD, 5);
        product.SoftDelete();
        var repository = new InMemoryProductWriteRepository([product]);
        var handler = new UpdateProductHandler(repository, new InMemoryUnitOfWork(), new UpdateProductValidator());

        var command = new UpdateProductCommand(product.Id, "Widget", null, "unit", 10m, Currency.USD, 5);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}

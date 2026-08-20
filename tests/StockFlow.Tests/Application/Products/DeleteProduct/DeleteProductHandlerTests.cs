using StockFlow.Application.Products.DeleteProduct;
using StockFlow.Domain.Products;

namespace StockFlow.Tests.Application.Products.DeleteProduct;

public class DeleteProductHandlerTests
{
    [Fact]
    public async Task DeleteProduct_WhenItExists_SoftDeletesProduct()
    {
        var product = Product.Create("SKU-100", "Widget", null, "unit", 10m, Currency.USD, 5);
        var repository = new InMemoryProductWriteRepository([product]);
        var handler = new DeleteProductHandler(repository, new InMemoryUnitOfWork());

        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(product.IsDeleted);
        Assert.NotNull(product.DeletedAt);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var repository = new InMemoryProductWriteRepository();
        var handler = new DeleteProductHandler(repository, new InMemoryUnitOfWork());

        var result = await handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductIsAlreadyDeleted_ReturnsNotFound()
    {
        var product = Product.Create("SKU-100", "Widget", null, "unit", 10m, Currency.USD, 5);
        product.SoftDelete();
        var repository = new InMemoryProductWriteRepository([product]);
        var handler = new DeleteProductHandler(repository, new InMemoryUnitOfWork());

        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}

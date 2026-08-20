using StockFlow.Application.Products.CreateProduct;
using StockFlow.Domain.Products;

namespace StockFlow.Tests.Application.Products.CreateProduct;

public class CreateProductHandlerTests
{
    private static CreateProductCommand ValidCommand(string sku = "SKU-100") => new(
        Sku: sku,
        Name: "Widget",
        Description: "A widget",
        UnitOfMeasure: "unit",
        Price: 10m,
        Currency: Currency.USD,
        MinimumStockThreshold: 5);

    [Fact]
    public async Task CreateProduct_WhenCommandIsValidAndSkuIsUnique_CreatesProduct()
    {
        var repository = new InMemoryProductWriteRepository();
        var handler = new CreateProductHandler(repository, new InMemoryUnitOfWork(), new CreateProductValidator());

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("SKU-100", result.Value.Sku);
    }

    [Fact]
    public async Task CreateProduct_WhenSkuAlreadyExists_ReturnsConflict()
    {
        var existingProduct = StockFlow.Domain.Products.Product.Create("SKU-100", "Existing", null, "unit", 1m, Currency.USD, 0);
        var repository = new InMemoryProductWriteRepository([existingProduct]);
        var handler = new CreateProductHandler(repository, new InMemoryUnitOfWork(), new CreateProductValidator());

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task CreateProduct_WhenCommandIsInvalid_ReturnsValidationError()
    {
        var repository = new InMemoryProductWriteRepository();
        var handler = new CreateProductHandler(repository, new InMemoryUnitOfWork(), new CreateProductValidator());

        var result = await handler.Handle(ValidCommand() with { Price = -5m }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}

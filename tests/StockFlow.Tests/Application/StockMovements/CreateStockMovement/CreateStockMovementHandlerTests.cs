using StockFlow.Application.StockMovements.CreateStockMovement;
using StockFlow.Domain.Locations;
using StockFlow.Domain.Products;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Tests.Application.StockMovements.CreateStockMovement;

public class CreateStockMovementHandlerTests
{
    private static Product NewProduct() => Product.Create("SKU-100", "Widget", null, "unit", 10m, Currency.USD, 5);

    private static Location NewLocation(string code = "WH-100", Country country = Country.US) =>
        Location.Create(code, "Main Warehouse", null, null, null, "CA", "Los Angeles", country);

    private static CreateStockMovementHandler HandlerWith(
        Product product,
        IEnumerable<Location> locations,
        InMemoryStockLevelRepository? stockLevelRepository = null,
        InMemoryStockMovementWriteRepository? stockMovementRepository = null) =>
        new(
            new InMemoryProductWriteRepository([product]),
            new InMemoryLocationWriteRepository(locations),
            stockMovementRepository ?? new InMemoryStockMovementWriteRepository(),
            stockLevelRepository ?? new InMemoryStockLevelRepository(),
            new InMemoryUnitOfWork(),
            new CreateStockMovementValidator());

    [Fact]
    public async Task CreateStockMovement_WithInType_IncreasesDestinationStock()
    {
        var product = NewProduct();
        var destination = NewLocation("WH-DEST");
        var stockLevelRepository = new InMemoryStockLevelRepository();
        var handler = HandlerWith(product, [destination], stockLevelRepository);

        var command = new CreateStockMovementCommand(product.Id, MovementType.In, 10, null, destination.Id, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stockLevel = await stockLevelRepository.GetOrCreateAsync(product.Id, destination.Id, CancellationToken.None);
        Assert.Equal(10, stockLevel.Quantity);
    }

    [Fact]
    public async Task CreateStockMovement_WithOutTypeAndEnoughStock_DecreasesSourceStock()
    {
        var product = NewProduct();
        var source = NewLocation("WH-SRC");
        var existingStockLevel = StockLevel.Create(product.Id, source.Id);
        existingStockLevel.Increase(20);
        var stockLevelRepository = new InMemoryStockLevelRepository([existingStockLevel]);
        var handler = HandlerWith(product, [source], stockLevelRepository);

        var command = new CreateStockMovementCommand(product.Id, MovementType.Out, 15, source.Id, null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stockLevel = await stockLevelRepository.GetOrCreateAsync(product.Id, source.Id, CancellationToken.None);
        Assert.Equal(5, stockLevel.Quantity);
    }

    [Fact]
    public async Task CreateStockMovement_WithOutTypeAndInsufficientStock_ReturnsConflict()
    {
        var product = NewProduct();
        var source = NewLocation("WH-SRC");
        var existingStockLevel = StockLevel.Create(product.Id, source.Id);
        existingStockLevel.Increase(5);
        var handler = HandlerWith(product, [source], new InMemoryStockLevelRepository([existingStockLevel]));

        var command = new CreateStockMovementCommand(product.Id, MovementType.Out, 10, source.Id, null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("StockMovements.InsufficientStock", result.Error.Code);
    }

    [Fact]
    public async Task CreateStockMovement_WithOutTypeAndNoExistingStockLevel_ReturnsConflict()
    {
        var product = NewProduct();
        var source = NewLocation("WH-SRC");
        var handler = HandlerWith(product, [source]);

        var command = new CreateStockMovementCommand(product.Id, MovementType.Out, 1, source.Id, null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task CreateStockMovement_WithTransfer_MovesStockBetweenLocations()
    {
        var product = NewProduct();
        var source = NewLocation("WH-SRC");
        var destination = NewLocation("WH-DEST");
        var existingStockLevel = StockLevel.Create(product.Id, source.Id);
        existingStockLevel.Increase(30);
        var stockLevelRepository = new InMemoryStockLevelRepository([existingStockLevel]);
        var handler = HandlerWith(product, [source, destination], stockLevelRepository);

        var command = new CreateStockMovementCommand(product.Id, MovementType.Transfer, 10, source.Id, destination.Id, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var sourceStock = await stockLevelRepository.GetOrCreateAsync(product.Id, source.Id, CancellationToken.None);
        var destinationStock = await stockLevelRepository.GetOrCreateAsync(product.Id, destination.Id, CancellationToken.None);
        Assert.Equal(20, sourceStock.Quantity);
        Assert.Equal(10, destinationStock.Quantity);
    }

    [Fact]
    public async Task CreateStockMovement_WithTransferAcrossDifferentCountries_ReturnsValidationError()
    {
        var product = NewProduct();
        var source = NewLocation("WH-SRC", Country.US);
        var destination = NewLocation("WH-DEST", Country.CR);
        var existingStockLevel = StockLevel.Create(product.Id, source.Id);
        existingStockLevel.Increase(30);
        var handler = HandlerWith(product, [source, destination], new InMemoryStockLevelRepository([existingStockLevel]));

        var command = new CreateStockMovementCommand(product.Id, MovementType.Transfer, 10, source.Id, destination.Id, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("StockMovements.CrossCountryTransfer", result.Error.Code);
    }

    [Fact]
    public async Task CreateStockMovement_WithAdjustmentIncrease_IncreasesStock()
    {
        var product = NewProduct();
        var location = NewLocation();
        var stockLevelRepository = new InMemoryStockLevelRepository();
        var handler = HandlerWith(product, [location], stockLevelRepository);

        var command = new CreateStockMovementCommand(product.Id, MovementType.Adjustment, 7, null, location.Id, MovementDirection.Increase);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stockLevel = await stockLevelRepository.GetOrCreateAsync(product.Id, location.Id, CancellationToken.None);
        Assert.Equal(7, stockLevel.Quantity);
    }

    [Fact]
    public async Task CreateStockMovement_WithAdjustmentDecreaseAndEnoughStock_DecreasesStock()
    {
        var product = NewProduct();
        var location = NewLocation();
        var existingStockLevel = StockLevel.Create(product.Id, location.Id);
        existingStockLevel.Increase(10);
        var stockLevelRepository = new InMemoryStockLevelRepository([existingStockLevel]);
        var handler = HandlerWith(product, [location], stockLevelRepository);

        var command = new CreateStockMovementCommand(product.Id, MovementType.Adjustment, 4, null, location.Id, MovementDirection.Decrease);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stockLevel = await stockLevelRepository.GetOrCreateAsync(product.Id, location.Id, CancellationToken.None);
        Assert.Equal(6, stockLevel.Quantity);
    }

    [Fact]
    public async Task CreateStockMovement_WithAdjustmentDecreaseAndInsufficientStock_ReturnsConflict()
    {
        var product = NewProduct();
        var location = NewLocation();
        var existingStockLevel = StockLevel.Create(product.Id, location.Id);
        existingStockLevel.Increase(2);
        var handler = HandlerWith(product, [location], new InMemoryStockLevelRepository([existingStockLevel]));

        var command = new CreateStockMovementCommand(product.Id, MovementType.Adjustment, 4, null, location.Id, MovementDirection.Decrease);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("StockMovements.InsufficientStock", result.Error.Code);
    }

    [Fact]
    public async Task CreateStockMovement_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var product = NewProduct();
        var destination = NewLocation();
        var handler = HandlerWith(product, [destination]);

        var command = new CreateStockMovementCommand(Guid.NewGuid(), MovementType.In, 10, null, destination.Id, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Products.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task CreateStockMovement_WhenDestinationLocationDoesNotExist_ReturnsNotFound()
    {
        var product = NewProduct();
        var handler = HandlerWith(product, []);

        var command = new CreateStockMovementCommand(product.Id, MovementType.In, 10, null, Guid.NewGuid(), null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Locations.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task CreateStockMovement_WhenSourceLocationDoesNotExist_ReturnsNotFound()
    {
        var product = NewProduct();
        var handler = HandlerWith(product, []);

        var command = new CreateStockMovementCommand(product.Id, MovementType.Out, 10, Guid.NewGuid(), null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Locations.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task CreateStockMovement_WhenCommandIsInvalid_ReturnsValidationError()
    {
        var product = NewProduct();
        var destination = NewLocation();
        var handler = HandlerWith(product, [destination]);

        var command = new CreateStockMovementCommand(product.Id, MovementType.In, 0, null, destination.Id, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("StockMovements.ValidationFailed", result.Error.Code);
    }

    [Fact]
    public async Task CreateStockMovement_WhenSuccessful_ReturnsMovementCarryingCreatedAtAndNoUpdatedFields()
    {
        var product = NewProduct();
        var destination = NewLocation();
        var handler = HandlerWith(product, [destination]);

        var command = new CreateStockMovementCommand(product.Id, MovementType.In, 10, null, destination.Id, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.CreatedAt <= DateTime.UtcNow);
        Assert.Null(result.Value.CreatedBy);
    }
}

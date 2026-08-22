using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Locations.Shared;
using StockFlow.Application.Products.Shared;
using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.Common;
using StockFlow.Domain.Locations;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Application.StockMovements.CreateStockMovement;

public sealed class CreateStockMovementHandler(
    IProductWriteRepository productRepository,
    ILocationWriteRepository locationRepository,
    IStockMovementWriteRepository stockMovementRepository,
    IStockLevelRepository stockLevelRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateStockMovementCommand> validator) : IRequestHandler<CreateStockMovementCommand, Result<StockMovementDto>>
{
    public async Task<Result<StockMovementDto>> Handle(CreateStockMovementCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<StockMovementDto>(StockMovementErrors.ValidationFailed(validationResult));
        }

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<StockMovementDto>(ProductErrors.NotFound(request.ProductId));
        }

        Location? sourceLocation = null;
        if (request.SourceLocationId is { } sourceLocationId)
        {
            sourceLocation = await locationRepository.GetByIdAsync(sourceLocationId, cancellationToken);
            if (sourceLocation is null)
            {
                return Result.Failure<StockMovementDto>(LocationErrors.NotFound(sourceLocationId));
            }
        }

        Location? destinationLocation = null;
        if (request.DestinationLocationId is { } destinationLocationId)
        {
            destinationLocation = await locationRepository.GetByIdAsync(destinationLocationId, cancellationToken);
            if (destinationLocation is null)
            {
                return Result.Failure<StockMovementDto>(LocationErrors.NotFound(destinationLocationId));
            }
        }

        if (request.Type == MovementType.Transfer && sourceLocation!.Address.Country != destinationLocation!.Address.Country)
        {
            return Result.Failure<StockMovementDto>(StockMovementErrors.CrossCountryTransfer());
        }

        // Only ever true for Out, Transfer (source half), and Adjustment/Decrease —
        // the only cases that can hit the negative-stock guard below.
        var isAdjustmentDecrease = request.Type == MovementType.Adjustment && request.Direction == MovementDirection.Decrease;
        var insufficientStockLocationId = isAdjustmentDecrease
            ? request.DestinationLocationId
            : request.SourceLocationId;

        try
        {
            if (sourceLocation is not null)
            {
                // Out and the source half of Transfer always decrease the source.
                var sourceStockLevel = await stockLevelRepository.GetOrCreateAsync(request.ProductId, sourceLocation.Id, cancellationToken);
                sourceStockLevel.Decrease(request.Quantity);
            }

            if (destinationLocation is not null)
            {
                var destinationStockLevel = await stockLevelRepository.GetOrCreateAsync(request.ProductId, destinationLocation.Id, cancellationToken);

                if (isAdjustmentDecrease)
                {
                    destinationStockLevel.Decrease(request.Quantity);
                }
                else
                {
                    // In, the destination half of Transfer, and Adjustment/Increase.
                    destinationStockLevel.Increase(request.Quantity);
                }
            }
        }
        catch (DomainValidationException)
        {
            // Insufficient stock at the affected location — an expected business
            // outcome here, not a truly unexpected error (see plan.md — Implementation).
            return Result.Failure<StockMovementDto>(StockMovementErrors.InsufficientStock(request.ProductId, insufficientStockLocationId!.Value));
        }

        var stockMovement = StockMovement.Create(
            request.ProductId,
            request.Type,
            request.Quantity,
            request.SourceLocationId,
            request.DestinationLocationId,
            request.Direction);

        try
        {
            await stockMovementRepository.AddAsync(stockMovement, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateCheckConstraintException)
        {
            // Translates a raw DB check-constraint violation (the race-condition
            // case a concurrent movement could slip past the in-memory guard
            // above) into the same business Result (see plan.md — Risks).
            return Result.Failure<StockMovementDto>(StockMovementErrors.InsufficientStock(request.ProductId, insufficientStockLocationId!.Value));
        }

        return Result.Success(stockMovement.ToDto());
    }
}

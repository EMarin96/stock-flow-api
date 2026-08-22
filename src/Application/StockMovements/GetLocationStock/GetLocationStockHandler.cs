using StockFlow.Application.Locations.Shared;
using StockFlow.Application.StockMovements.Shared;

namespace StockFlow.Application.StockMovements.GetLocationStock;

public sealed class GetLocationStockHandler(
    ILocationReadRepository locationReadRepository,
    IStockLevelReadRepository stockLevelReadRepository,
    IValidator<GetLocationStockQuery> validator) : IRequestHandler<GetLocationStockQuery, Result<PagedResult<LocationStockDto>>>
{
    public async Task<Result<PagedResult<LocationStockDto>>> Handle(GetLocationStockQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<PagedResult<LocationStockDto>>(StockMovementErrors.ValidationFailed(validationResult));
        }

        var location = await locationReadRepository.GetByIdAsync(request.LocationId, cancellationToken);
        if (location is null)
        {
            return Result.Failure<PagedResult<LocationStockDto>>(LocationErrors.NotFound(request.LocationId));
        }

        var pagedStock = await stockLevelReadRepository.GetPagedByLocationAsync(
            request.LocationId,
            request.ProductNameFilter,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(pagedStock);
    }
}

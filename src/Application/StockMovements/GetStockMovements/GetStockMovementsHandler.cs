using StockFlow.Application.StockMovements.Shared;

namespace StockFlow.Application.StockMovements.GetStockMovements;

public sealed class GetStockMovementsHandler(
    IStockMovementReadRepository repository,
    IValidator<GetStockMovementsQuery> validator) : IRequestHandler<GetStockMovementsQuery, Result<PagedResult<StockMovementDto>>>
{
    public async Task<Result<PagedResult<StockMovementDto>>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<PagedResult<StockMovementDto>>(StockMovementErrors.ValidationFailed(validationResult));
        }

        var pagedMovements = await repository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.ProductId,
            request.LocationId,
            request.Type,
            cancellationToken);

        return Result.Success(pagedMovements);
    }
}

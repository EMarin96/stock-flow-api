using StockFlow.Application.Locations.Shared;

namespace StockFlow.Application.Locations.GetLocations;

public sealed class GetLocationsHandler(
    ILocationReadRepository repository,
    IValidator<GetLocationsQuery> validator) : IRequestHandler<GetLocationsQuery, Result<PagedResult<LocationDto>>>
{
    public async Task<Result<PagedResult<LocationDto>>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<PagedResult<LocationDto>>(LocationErrors.ValidationFailed(validationResult));
        }

        var pagedLocations = await repository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        return Result.Success(pagedLocations);
    }
}

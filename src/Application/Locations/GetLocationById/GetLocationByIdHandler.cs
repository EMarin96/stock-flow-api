using StockFlow.Application.Locations.Shared;

namespace StockFlow.Application.Locations.GetLocationById;

public sealed class GetLocationByIdHandler(ILocationReadRepository repository)
    : IRequestHandler<GetLocationByIdQuery, Result<LocationDto>>
{
    public async Task<Result<LocationDto>> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var location = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (location is null)
        {
            return Result.Failure<LocationDto>(LocationErrors.NotFound(request.Id));
        }

        return Result.Success(location);
    }
}

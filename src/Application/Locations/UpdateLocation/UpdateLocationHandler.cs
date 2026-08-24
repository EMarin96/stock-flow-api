using StockFlow.Application.Common.Security;
using StockFlow.Application.Locations.Shared;

namespace StockFlow.Application.Locations.UpdateLocation;

public sealed class UpdateLocationHandler(
    ILocationWriteRepository repository,
    ICountryReferenceDataService referenceDataService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IValidator<UpdateLocationCommand> validator) : IRequestHandler<UpdateLocationCommand, Result<LocationDto>>
{
    public async Task<Result<LocationDto>> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<LocationDto>(LocationErrors.ValidationFailed(validationResult));
        }

        var location = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (location is null)
        {
            return Result.Failure<LocationDto>(LocationErrors.NotFound(request.Id));
        }

        // Country/State/City are unconditionally required — Domain cannot
        // confirm State/City are *real* values (no I/O), so that check happens
        // here (see plan.md — Decisions).
        var referenceDataError = await AddressReferenceDataValidator.ValidateAsync(
            referenceDataService,
            request.Country,
            request.State,
            request.City,
            cancellationToken);

        if (referenceDataError is not null)
        {
            return Result.Failure<LocationDto>(referenceDataError);
        }

        location.Update(
            request.Name,
            request.AddressLine1,
            request.AddressLine2,
            request.AddressLine3,
            request.State,
            request.City,
            request.Country,
            currentUserService.UserId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(location.ToDto());
    }
}

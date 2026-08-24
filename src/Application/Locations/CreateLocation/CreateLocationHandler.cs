using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Common.Security;
using StockFlow.Application.Locations.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Application.Locations.CreateLocation;

public sealed class CreateLocationHandler(
    ILocationWriteRepository repository,
    ICountryReferenceDataService referenceDataService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IValidator<CreateLocationCommand> validator) : IRequestHandler<CreateLocationCommand, Result<LocationDto>>
{
    public async Task<Result<LocationDto>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<LocationDto>(LocationErrors.ValidationFailed(validationResult));
        }

        // Pre-check for a clean business error; the DB unique index on Code is the
        // final safety net against the race condition between this check and the
        // insert below (see plan.md — Risks).
        var codeAlreadyExists = await repository.CodeExistsAsync(request.Code, cancellationToken);
        if (codeAlreadyExists)
        {
            return Result.Failure<LocationDto>(LocationErrors.CodeAlreadyExists(request.Code));
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

        var location = Location.Create(
            request.Code,
            request.Name,
            request.AddressLine1,
            request.AddressLine2,
            request.AddressLine3,
            request.State,
            request.City,
            request.Country,
            currentUserService.UserId);

        try
        {
            await repository.AddAsync(location, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateUniqueConstraintException)
        {
            // Translates a raw DB unique-constraint violation (the race-condition
            // case) into the same business Result the pre-check above returns.
            return Result.Failure<LocationDto>(LocationErrors.CodeAlreadyExists(request.Code));
        }

        return Result.Success(location.ToDto());
    }
}

using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Countries.Shared;
using StockFlow.Application.Locations.Shared;

namespace StockFlow.Application.Countries.GetStates;

public sealed class GetStatesHandler(ICountryReferenceDataService referenceDataService)
    : IRequestHandler<GetStatesQuery, Result<IReadOnlyList<StateDto>>>
{
    public async Task<Result<IReadOnlyList<StateDto>>> Handle(GetStatesQuery request, CancellationToken cancellationToken)
    {
        if (!CountryCodeParser.TryParse(request.CountryCode, out var country))
        {
            return Result.Failure<IReadOnlyList<StateDto>>(CountryErrors.InvalidCountryCode(request.CountryCode));
        }

        IReadOnlyList<StateInfo> states;
        try
        {
            states = await referenceDataService.GetStatesAsync(country, cancellationToken);
        }
        catch (ReferenceDataUnavailableException)
        {
            return Result.Failure<IReadOnlyList<StateDto>>(CountryErrors.ReferenceDataUnavailable());
        }

        IReadOnlyList<StateDto> result = states.Select(state => new StateDto(state.Iso2, state.Name)).ToList();

        return Result.Success(result);
    }
}

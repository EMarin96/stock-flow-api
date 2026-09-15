using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Countries.Shared;
using StockFlow.Application.Locations.Shared;

namespace StockFlow.Application.Countries.GetCities;

public sealed class GetCitiesHandler(ICountryReferenceDataService referenceDataService)
    : IRequestHandler<GetCitiesQuery, Result<IReadOnlyList<CityDto>>>
{
    public async Task<Result<IReadOnlyList<CityDto>>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
    {
        if (!CountryCodeParser.TryParse(request.CountryCode, out var country))
        {
            return Result.Failure<IReadOnlyList<CityDto>>(CountryErrors.InvalidCountryCode(request.CountryCode));
        }

        IReadOnlyList<StateInfo> states;
        try
        {
            states = await referenceDataService.GetStatesAsync(country, cancellationToken);
        }
        catch (ReferenceDataUnavailableException)
        {
            return Result.Failure<IReadOnlyList<CityDto>>(CountryErrors.ReferenceDataUnavailable());
        }

        var matchedState = states.FirstOrDefault(
            candidate => string.Equals(candidate.Iso2, request.StateIso2, StringComparison.OrdinalIgnoreCase));
        if (matchedState is null)
        {
            return Result.Failure<IReadOnlyList<CityDto>>(CountryErrors.InvalidState(country, request.StateIso2));
        }

        IReadOnlyList<CityInfo> cities;
        try
        {
            cities = await referenceDataService.GetCitiesAsync(country, matchedState.Iso2, cancellationToken);
        }
        catch (ReferenceDataUnavailableException)
        {
            return Result.Failure<IReadOnlyList<CityDto>>(CountryErrors.ReferenceDataUnavailable());
        }

        IReadOnlyList<CityDto> result = cities.Select(city => new CityDto(city.Name)).ToList();

        return Result.Success(result);
    }
}

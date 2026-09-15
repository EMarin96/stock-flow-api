using StockFlow.Application.Countries.Shared;
using StockFlow.Domain.Locations;

namespace StockFlow.Application.Countries.GetCountries;

public sealed class GetCountriesHandler : IRequestHandler<GetCountriesQuery, Result<IReadOnlyList<CountryDto>>>
{
    public Task<Result<IReadOnlyList<CountryDto>>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<CountryDto> countries = Enum.GetValues<Country>()
            .Select(country => new CountryDto(country.ToString()))
            .ToList();

        return Task.FromResult(Result.Success(countries));
    }
}

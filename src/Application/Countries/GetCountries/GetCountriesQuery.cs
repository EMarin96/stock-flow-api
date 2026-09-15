using StockFlow.Application.Countries.Shared;

namespace StockFlow.Application.Countries.GetCountries;

public sealed record GetCountriesQuery : IRequest<Result<IReadOnlyList<CountryDto>>>;

using StockFlow.Application.Countries.Shared;

namespace StockFlow.Application.Countries.GetCities;

public sealed record GetCitiesQuery(string CountryCode, string StateIso2) : IRequest<Result<IReadOnlyList<CityDto>>>;

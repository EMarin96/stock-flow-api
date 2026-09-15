using StockFlow.Application.Countries.Shared;

namespace StockFlow.Application.Countries.GetStates;

public sealed record GetStatesQuery(string CountryCode) : IRequest<Result<IReadOnlyList<StateDto>>>;

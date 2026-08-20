namespace StockFlow.Application.Locations.DeleteLocation;

public sealed record DeleteLocationCommand(Guid Id) : IRequest<Result>;

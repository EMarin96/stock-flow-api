namespace StockFlow.Application.Common.Mediator;

/// <summary>
/// Marker interface for a command or query dispatched through the in-house
/// Mediator. Commands and queries alike return a <c>TResponse</c> — typically
/// a <see cref="Results.Result"/> or <see cref="Results.Result{TValue}"/>.
/// </summary>
public interface IRequest<TResponse>
{
}

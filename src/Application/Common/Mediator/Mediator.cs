using System.Collections.Concurrent;

namespace StockFlow.Application.Common.Mediator;

/// <summary>
/// In-house Mediator implementation. Resolves the matching
/// <see cref="IRequestHandler{TRequest,TResponse}"/> for a given request via DI and
/// invokes it. Used instead of the MediatR library, which is excluded per its paid
/// license (see tech-stack.md hard limits).
/// </summary>
public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var handlerType = HandlerTypeCache.GetOrAdd(
            requestType,
            type => typeof(IRequestHandler<,>).MakeGenericType(type, typeof(TResponse)));

        var handler = serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for request type '{requestType.Name}'.");

        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))
            ?? throw new InvalidOperationException($"Handler for '{requestType.Name}' does not expose a Handle method.");

        return (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;
    }
}

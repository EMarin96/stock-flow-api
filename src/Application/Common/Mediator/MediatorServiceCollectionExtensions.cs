using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace StockFlow.Application.Common.Mediator;

public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers the in-house Mediator and scans the given assemblies for
    /// closed <see cref="IRequestHandler{TRequest,TResponse}"/> implementations.
    /// </summary>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IMediator, Mediator>();

        var handlerInterfaceType = typeof(IRequestHandler<,>);

        foreach (var assembly in assemblies)
        {
            var handlerImplementations = assembly.GetTypes()
                .Where(type => type is { IsAbstract: false, IsInterface: false })
                .SelectMany(type => type.GetInterfaces(), (implementation, @interface) => (implementation, @interface))
                .Where(pair => pair.@interface.IsGenericType && pair.@interface.GetGenericTypeDefinition() == handlerInterfaceType);

            foreach (var (implementation, @interface) in handlerImplementations)
            {
                services.AddScoped(@interface, implementation);
            }
        }

        return services;
    }
}

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace StockFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.GetExecutingAssembly();

        services.AddMediator(applicationAssembly);
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Application.Engines.Scope;

public static class ScopeServiceCollectionExtensions
{
    /// <summary>Registers every non-abstract IScopeDimensionProvider / IBasePopulationProvider in the
    /// assembly as scoped. Each owning module calls this for its own assembly; the ScopeEngine then
    /// discovers new dimensions automatically — payroll never changes.</summary>
    public static IServiceCollection AddScopeProvidersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        foreach (var t in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false }))
        {
            if (typeof(IScopeDimensionProvider).IsAssignableFrom(t))
                services.AddScoped(typeof(IScopeDimensionProvider), t);
            if (typeof(IBasePopulationProvider).IsAssignableFrom(t))
                services.AddScoped(typeof(IBasePopulationProvider), t);
        }
        return services;
    }
}

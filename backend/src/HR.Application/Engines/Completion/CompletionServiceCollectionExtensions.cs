using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Application.Engines.Completion;

public static class CompletionServiceCollectionExtensions
{
    /// <summary>
    /// Registers every non-abstract <see cref="IEffectExecutor"/> in the given assembly as a scoped
    /// service. Each module calls this for its own assembly, so the Completion Engine discovers new
    /// executors automatically — adding a module never requires editing the platform.
    /// </summary>
    public static IServiceCollection AddEffectExecutorsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var executorTypes = assembly.GetTypes()
            .Where(t => typeof(IEffectExecutor).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false });

        foreach (var type in executorTypes)
            services.AddScoped(typeof(IEffectExecutor), type);

        return services;
    }
}

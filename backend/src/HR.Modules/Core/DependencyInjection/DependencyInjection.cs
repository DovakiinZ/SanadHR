using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreModule(this IServiceCollection services)
    {
        return services;
    }
}

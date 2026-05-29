using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.ESS;

public static class DependencyInjection
{
    public static IServiceCollection AddESSModule(this IServiceCollection services)
    {
        return services;
    }
}

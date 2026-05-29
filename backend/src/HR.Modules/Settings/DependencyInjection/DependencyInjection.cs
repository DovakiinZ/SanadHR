using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Settings;

public static class DependencyInjection
{
    public static IServiceCollection AddSettingsModule(this IServiceCollection services)
    {
        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Dashboards;

public static class DependencyInjection
{
    public static IServiceCollection AddDashboardsModule(this IServiceCollection services)
    {
        return services;
    }
}

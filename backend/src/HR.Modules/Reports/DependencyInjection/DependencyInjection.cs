using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Reports;

public static class DependencyInjection
{
    public static IServiceCollection AddReportsModule(this IServiceCollection services)
    {
        return services;
    }
}

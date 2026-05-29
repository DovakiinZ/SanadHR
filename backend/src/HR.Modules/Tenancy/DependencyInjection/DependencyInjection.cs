using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Tenancy;

public static class DependencyInjection
{
    public static IServiceCollection AddTenancyModule(this IServiceCollection services)
    {
        return services;
    }
}

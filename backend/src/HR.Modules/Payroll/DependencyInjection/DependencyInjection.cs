using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Payroll;

public static class DependencyInjection
{
    public static IServiceCollection AddPayrollModule(this IServiceCollection services)
    {
        return services;
    }
}

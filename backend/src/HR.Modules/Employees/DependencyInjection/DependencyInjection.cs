using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Employees;

public static class DependencyInjection
{
    public static IServiceCollection AddEmployeesModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        return services;
    }
}

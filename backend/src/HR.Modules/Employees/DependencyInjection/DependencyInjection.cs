using System.Reflection;
using FluentValidation;
using HR.Application.Engines.Scope;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Employees;

public static class DependencyInjection
{
    public static IServiceCollection AddEmployeesModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        // Register all IScopeDimensionProvider / IBasePopulationProvider implementations in this assembly.
        services.AddScopeProvidersFromAssembly(typeof(HR.Modules.Employees.Scope.DepartmentScopeProvider).Assembly);
        return services;
    }
}

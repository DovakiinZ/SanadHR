using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Tasks;

public static class DependencyInjection
{
    public static IServiceCollection AddTasksModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        return services;
    }
}

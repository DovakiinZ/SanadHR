using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Workflows;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkflowsModule(this IServiceCollection services)
    {
        return services;
    }
}

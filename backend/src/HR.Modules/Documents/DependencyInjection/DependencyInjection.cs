using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Documents;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentsModule(this IServiceCollection services)
    {
        return services;
    }
}

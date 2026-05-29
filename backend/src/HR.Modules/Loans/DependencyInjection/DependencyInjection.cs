using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Loans;

public static class DependencyInjection
{
    public static IServiceCollection AddLoansModule(this IServiceCollection services)
    {
        return services;
    }
}

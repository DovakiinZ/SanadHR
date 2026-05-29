using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Expenses;

public static class DependencyInjection
{
    public static IServiceCollection AddExpensesModule(this IServiceCollection services)
    {
        return services;
    }
}

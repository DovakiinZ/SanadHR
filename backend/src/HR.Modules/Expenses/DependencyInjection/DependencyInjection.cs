using System.Reflection;
using HR.Application.Engines.Completion;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Expenses;

public static class DependencyInjection
{
    public static IServiceCollection AddExpensesModule(this IServiceCollection services)
    {
        // Completion effect this module owns (Expense.CreateClaim).
        services.AddEffectExecutorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}

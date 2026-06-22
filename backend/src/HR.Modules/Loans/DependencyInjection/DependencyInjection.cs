using System.Reflection;
using HR.Application.Engines.Completion;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Loans;

public static class DependencyInjection
{
    public static IServiceCollection AddLoansModule(this IServiceCollection services)
    {
        // Completion effect this module owns (Loan.Create).
        services.AddEffectExecutorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}

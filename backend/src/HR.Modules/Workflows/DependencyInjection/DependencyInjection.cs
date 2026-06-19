using System.Reflection;
using HR.Modules.Workflows.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Workflows;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkflowsModule(this IServiceCollection services)
    {
        // CQRS handlers (commands + queries) for this module.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Step handlers — registered as a collection; the runner resolves the right one by step type.
        // Adding a new step type is just a new IWorkflowStepHandler registration here (Open/Closed).
        services.AddScoped<IWorkflowStepHandler, ApprovalStepHandler>();
        services.AddScoped<IWorkflowStepHandler, EmailActionHandler>();
        services.AddScoped<IWorkflowStepHandler, ConditionStepHandler>();

        // Execution engine + graph validator + e-mail delivery.
        services.AddScoped<IWorkflowRunner, WorkflowRunner>();
        services.AddScoped<IWorkflowGraphValidator, WorkflowGraphValidator>();
        services.AddScoped<IWorkflowEmailSender, QueueWorkflowEmailSender>();

        return services;
    }
}

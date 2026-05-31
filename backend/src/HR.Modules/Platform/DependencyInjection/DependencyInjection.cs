using System.Reflection;
using HR.Application.Engines.Audit;
using HR.Application.Engines.Automation;
using HR.Application.Engines.Permissions;
using HR.Application.Engines.Timeline;
using HR.Application.Engines.Tokens;
using HR.Application.Engines.Workflows;
using HR.Infrastructure.Engines.Audit;
using HR.Infrastructure.Engines.Automation;
using HR.Infrastructure.Engines.Permissions;
using HR.Infrastructure.Engines.Timeline;
using HR.Infrastructure.Engines.Tokens;
using HR.Infrastructure.Engines.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Platform;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Engine services
        services.AddScoped<IPermissionEvaluator, PermissionEvaluator>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        services.AddScoped<IAutomationEngine, AutomationEngine>();
        services.AddScoped<IAuditEngine, AuditEngine>();
        services.AddScoped<ITimelineEngine, TimelineEngine>();
        services.AddScoped<ITokenResolver, TokenResolver>();

        return services;
    }
}

using HR.Application.Common.Interfaces;
using HR.Modules.Workflows.Execution;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Workflows.Tests;

/// <summary>A deterministic current-user used across tests (single fixed tenant + user).</summary>
public sealed class FakeCurrentUserService : ICurrentUserService
{
    public static readonly Guid Tenant = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid User = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public Guid UserId => User;
    public Guid TenantId => Tenant;
    public string? Email => "tester@hrcloud.local";
    public IReadOnlyList<string> Permissions { get; } = new[] { "Workflows.View", "Workflows.Create", "Workflows.Edit", "Workflows.Delete" };
    public bool IsAuthenticated => true;
}

/// <summary>Records what would have been e-mailed, instead of touching any real delivery mechanism.</summary>
public sealed class FakeEmailSender : IWorkflowEmailSender
{
    public List<(string To, string Subject, string Body)> Sent { get; } = new();

    public Task SendAsync(string toEmail, string subject, string body, Guid? relatedRequestId, CancellationToken ct)
    {
        Sent.Add((toEmail, subject, body));
        return Task.CompletedTask;
    }
}

/// <summary>Builds the in-memory pieces a handler test needs.</summary>
public static class TestHarness
{
    /// <summary>
    /// A fresh context bound to a named in-memory database. Tests pass the same name to several
    /// calls so each operation gets its own context (like a scoped per-request DbContext in prod)
    /// while sharing the underlying data — this avoids change-tracker carryover between save cycles.
    /// </summary>
    public static ApplicationDbContext NewContext(ICurrentUserService user, string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;
        return new ApplicationDbContext(options, user);
    }

    public static string NewDbName() => $"wf-tests-{Guid.NewGuid()}";

    /// <summary>The real runner wired with all three real step handlers (Approval/Action/Condition).</summary>
    public static WorkflowRunner NewRunner(ApplicationDbContext ctx, IWorkflowEmailSender email) => new(new IWorkflowStepHandler[]
    {
        new ApprovalStepHandler(),
        new EmailActionHandler(email),
        new ConditionStepHandler()
    }, ctx);
}

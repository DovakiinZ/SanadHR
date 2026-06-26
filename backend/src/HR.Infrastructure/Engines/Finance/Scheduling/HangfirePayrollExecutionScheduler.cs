using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using Hangfire;

namespace HR.Infrastructure.Engines.Finance.Scheduling;

/// <summary>Enqueues execution as a durable Hangfire background job, capturing the caller's tenant/user so
/// the worker can re-establish the ambient context. Used when "Hangfire:Enabled" is configured — large
/// runs then process off the request thread and survive process restarts.</summary>
public sealed class HangfirePayrollExecutionScheduler : IPayrollExecutionScheduler
{
    private readonly IBackgroundJobClient _client;
    private readonly ICurrentUserService _currentUser;

    public HangfirePayrollExecutionScheduler(IBackgroundJobClient client, ICurrentUserService currentUser)
    {
        _client = client;
        _currentUser = currentUser;
    }

    public Task<string?> EnqueueAsync(Guid runId, CancellationToken ct = default)
    {
        var tenantId = _currentUser.TenantId;
        var userId = _currentUser.IsAuthenticated ? _currentUser.UserId : (Guid?)null;
        var email = _currentUser.Email;
        var jobId = _client.Enqueue<PayrollExecutionJob>(j => j.RunAsync(runId, tenantId, userId, email));
        return Task.FromResult<string?>(jobId);
    }
}

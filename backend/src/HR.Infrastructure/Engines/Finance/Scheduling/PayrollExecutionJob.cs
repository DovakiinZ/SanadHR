using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using Hangfire;

namespace HR.Infrastructure.Engines.Finance.Scheduling;

/// <summary>The Hangfire job that runs a payroll execution on a background worker. It re-establishes the
/// tenant via the ambient context (there is no HTTP principal here) and delegates to the orchestrator,
/// which is itself resumable — so Hangfire's automatic retries continue a partially-completed run rather
/// than restarting it.</summary>
public sealed class PayrollExecutionJob
{
    private readonly IPayrollExecutionEngine _engine;
    private readonly IBackgroundExecutionContext _background;

    public PayrollExecutionJob(IPayrollExecutionEngine engine, IBackgroundExecutionContext background)
    {
        _engine = engine;
        _background = background;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task RunAsync(Guid runId, Guid tenantId, Guid? userId, string? email)
    {
        using (_background.Begin(tenantId, userId, email))
        {
            await _engine.ExecuteAsync(runId, CancellationToken.None);
        }
    }
}

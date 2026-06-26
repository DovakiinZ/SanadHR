using HR.Application.Engines.Finance;

namespace HR.Infrastructure.Engines.Finance.Scheduling;

/// <summary>Default scheduler: runs execution inline within the current request/scope (tenant already
/// established). Suitable for small/medium runs; swap in the Hangfire scheduler for large or distributed
/// workloads via configuration.</summary>
public sealed class InProcessPayrollExecutionScheduler : IPayrollExecutionScheduler
{
    private readonly IPayrollExecutionEngine _engine;

    public InProcessPayrollExecutionScheduler(IPayrollExecutionEngine engine) => _engine = engine;

    public async Task<string?> EnqueueAsync(Guid runId, CancellationToken ct = default)
    {
        await _engine.ExecuteAsync(runId, ct);
        return null;
    }
}

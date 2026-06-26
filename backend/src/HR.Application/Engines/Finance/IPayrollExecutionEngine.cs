using HR.Domain.Engines.Finance.Entities;

namespace HR.Application.Engines.Finance;

/// <summary>The batch orchestrator. Executes an approved run by posting every employee's payslip to the
/// immutable ledger, tracking each as a <see cref="PayrollRunItem"/> so the work is concurrent, retriable
/// and resumable. Re-invoking a partially-completed or failed run continues from where it stopped and
/// never double-posts. Designed to run in-process or on a distributed worker (Hangfire).</summary>
public interface IPayrollExecutionEngine
{
    Task<PayrollRun> ExecuteAsync(Guid runId, CancellationToken ct = default);
}

/// <summary>Schedules a run's execution. The in-process implementation runs it inline; the Hangfire
/// implementation enqueues a durable background job (propagating the tenant), so a large run doesn't
/// block the request and survives process restarts.</summary>
public interface IPayrollExecutionScheduler
{
    /// <summary>Queue (or run) execution of an approved run. Returns a job id when applicable.</summary>
    Task<string?> EnqueueAsync(Guid runId, CancellationToken ct = default);
}

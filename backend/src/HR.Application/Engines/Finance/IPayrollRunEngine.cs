using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;

namespace HR.Application.Engines.Finance;

/// <summary>Orchestrates the payroll run lifecycle through the state machine, recording every transition.
/// Calculate freezes an immutable per-employee snapshot (the Snapshot Engine); Validate gates progress;
/// Approve locks the run's figures. Execution/ledger-posting (Approved → Completed) arrives with the
/// batch orchestrator in the next pass.</summary>
public interface IPayrollRunEngine
{
    /// <summary>Create a Draft run from a definition's currently published version + pinned rule-set version.</summary>
    Task<PayrollRun> CreateAsync(Guid payrollDefinitionId, PayrollPeriod period, CancellationToken ct = default);

    /// <summary>Compute every employee and persist immutable payslip snapshots; Draft → Preview.</summary>
    Task<PayrollRun> CalculateAsync(Guid runId, CancellationToken ct = default);

    /// <summary>Run all validators against the calculated run; on success Preview → Validated and the
    /// report is frozen onto the run. Returns the report either way.</summary>
    Task<ValidationReport> ValidateAsync(Guid runId, CancellationToken ct = default);

    /// <summary>Validated → PendingApproval.</summary>
    Task<PayrollRun> SubmitForApprovalAsync(Guid runId, CancellationToken ct = default);

    /// <summary>PendingApproval → Approved. The snapshot is now immutable.</summary>
    Task<PayrollRun> ApproveAsync(Guid runId, CancellationToken ct = default);

    /// <summary>Move a run to Cancelled (allowed from any pre-execution state).</summary>
    Task<PayrollRun> CancelAsync(Guid runId, string reason, CancellationToken ct = default);
}

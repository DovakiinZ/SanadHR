using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Finance.Entities;

/// <summary>One unit of work in a batch payroll execution: a single employee's payslip being posted to
/// the ledger. Tracking each employee as its own row is what makes a run concurrent, retriable and
/// resumable — a re-run skips items already <see cref="PayrollRunItemState.Completed"/> and retries the
/// <see cref="PayrollRunItemState.Failed"/> ones, so a 5,000-employee run can recover from a crash
/// without double-paying anyone.</summary>
public class PayrollRunItem : TenantEntity
{
    public Guid PayrollRunId { get; set; }
    public PayrollRun? Run { get; set; }

    public Guid EmployeeId { get; set; }
    public Guid PayslipId { get; set; }

    public PayrollRunItemState State { get; set; } = PayrollRunItemState.Pending;

    public int Attempts { get; set; }
    public string? Error { get; set; }

    /// <summary>How many ledger entries this item posted (for reconciliation).</summary>
    public int LedgerEntryCount { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

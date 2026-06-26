using HR.Domain.Common;

namespace HR.Domain.Engines.Finance.Entities;

/// <summary>An immutable per-employee snapshot produced when a payroll run is calculated. It freezes the
/// exact input facts and computed components for one employee, so a historical payslip is reproducible
/// and never affected by later edits to the employee record. Together with the run's pinned definition
/// and rule-set versions, this is the Snapshot Engine's unit of record.</summary>
public class PayrollPayslip : TenantEntity
{
    public Guid PayrollRunId { get; set; }
    public PayrollRun? Run { get; set; }

    public Guid EmployeeId { get; set; }

    /// <summary>Denormalized employee identity at run time (immutable snapshot).</summary>
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;

    public string Currency { get; set; } = "SAR";

    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetAmount { get; set; }

    /// <summary>The exact input facts the calculation ran on (jsonb).</summary>
    public string? FactsJson { get; set; }

    /// <summary>The computed components and their amounts/order (jsonb).</summary>
    public string? ComponentsJson { get; set; }

    /// <summary>Per-employee warnings raised during calculation (jsonb).</summary>
    public string? WarningsJson { get; set; }

    /// <summary>Set once the payslip's amounts have been posted to the financial ledger (Pass 3).</summary>
    public bool LedgerPosted { get; set; }
    public DateTime? LedgerPostedAt { get; set; }
}

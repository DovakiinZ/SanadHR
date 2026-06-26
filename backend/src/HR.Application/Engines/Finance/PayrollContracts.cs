using HR.Domain.Engines.Finance;

namespace HR.Application.Engines.Finance;

/// <summary>The resolved inputs for one employee for a pay period: identity, currency and the fact bag
/// fed to the rule engine (BasicSalary, TotalAllowances, GosiRate, …).</summary>
public sealed record EmployeePayrollInput
{
    public Guid EmployeeId { get; init; }
    public string EmployeeNumber { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public string Currency { get; init; } = "SAR";
    public IReadOnlyDictionary<string, object?> Facts { get; init; } = new Dictionary<string, object?>();

    /// <summary>Whether attendance data exists for this employee in the period (drives a validation check).</summary>
    public bool HasAttendanceData { get; init; }
}

/// <summary>The computed payroll outcome for one employee: the inputs it ran on, the rule-set evaluation
/// (components + gross/deductions/net) and any per-employee warnings raised during calculation.</summary>
public sealed record EmployeePayrollResult
{
    public required EmployeePayrollInput Input { get; init; }
    public required RuleSetEvaluation Evaluation { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = new List<string>();

    public Guid EmployeeId => Input.EmployeeId;
    public decimal Gross => Evaluation.GrossEarnings;
    public decimal Deductions => Evaluation.TotalDeductions;
    public decimal Net => Evaluation.NetAmount;
}

/// <summary>One line of a preview: an employee and their headline figures, plus whether they have any
/// blocking errors.</summary>
public sealed record PayrollPreviewLine(
    Guid EmployeeId,
    string EmployeeNumber,
    string EmployeeName,
    decimal Gross,
    decimal Deductions,
    decimal Net,
    bool HasErrors);

/// <summary>The result of a preview: aggregate counts/totals, the validation report (errors + warnings)
/// and the per-employee lines — all computed without writing anything to the database.</summary>
public sealed record PayrollPreview
{
    public int EmployeeCount { get; init; }
    public decimal GrossTotal { get; init; }
    public decimal DeductionTotal { get; init; }
    public decimal NetTotal { get; init; }
    public string Currency { get; init; } = "SAR";
    public ValidationReport Validation { get; init; } = ValidationReport.Empty;
    public IReadOnlyList<PayrollPreviewLine> Lines { get; init; } = new List<PayrollPreviewLine>();
}

using HR.Domain.Common;

namespace HR.Domain.Engines.Finance.Entities;

/// <summary>A frozen snapshot of one employee resolved into a payroll run at creation time. Future
/// organizational changes never alter a historical run because the run reads this snapshot, not live data.</summary>
public class PayrollRunPopulation : TenantEntity
{
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? JobTitleId { get; set; }
    public Guid? PaymentMethodId { get; set; }

    public bool IsIncluded { get; set; } = true;
    /// <summary>Null when included. Sub-project 1 sets "ExcludedByScope"; sub-project 3 adds validity reasons.</summary>
    public string? ExclusionReasonCode { get; set; }
}

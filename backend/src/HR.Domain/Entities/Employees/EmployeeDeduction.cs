using HR.Domain.Common;

namespace HR.Modules.Employees.Entities;

/// <summary>
/// A per-employee salary DEDUCTION (loan repayment / absence / penalty …). References a
/// DeductionType master-data item (governed, no free text) and stores the effective
/// amount for this employee. Negative component of net salary. GOSI is computed
/// separately from the company rate, not stored here.
/// </summary>
public class EmployeeDeduction : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid DeductionTypeId { get; set; }  // master-data DeductionType
    public decimal Amount { get; set; }
    public bool IsActive { get; set; } = true;
}

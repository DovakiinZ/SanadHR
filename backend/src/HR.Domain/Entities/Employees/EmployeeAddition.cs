using HR.Domain.Common;

namespace HR.Modules.Employees.Entities;

/// <summary>
/// A per-employee salary ADDITION (bonus / overtime / commission …). References an
/// AdditionType master-data item (governed, no free text) and stores the effective
/// amount for this employee. Positive component of net salary.
/// </summary>
public class EmployeeAddition : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid AdditionTypeId { get; set; }   // master-data AdditionType
    public decimal Amount { get; set; }
    public bool IsActive { get; set; } = true;
}

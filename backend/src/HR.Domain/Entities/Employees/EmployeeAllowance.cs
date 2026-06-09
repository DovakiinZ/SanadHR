using HR.Domain.Common;

namespace HR.Modules.Employees.Entities;

/// <summary>
/// A per-employee allowance value. References an AllowanceType master-data item
/// (governed, no free text) and stores the effective amount for this employee —
/// an override of the allowance type's default when overrides are allowed.
/// </summary>
public class EmployeeAllowance : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid AllowanceTypeId { get; set; }   // master-data AllowanceType
    public decimal Amount { get; set; }         // effective value (fixed amount or percentage, per the type)
    public bool IsActive { get; set; } = true;
}

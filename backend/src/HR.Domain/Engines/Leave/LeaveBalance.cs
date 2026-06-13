using HR.Domain.Common;

namespace HR.Domain.Engines.Leave;

/// <summary>An employee's balance for a given leave type and year. LeaveType is an object reference.</summary>
public class LeaveBalance : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }          // MasterDataItem (ObjectType=LeaveType)
    public int Year { get; set; }
    public decimal EntitledDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal CarriedForwardDays { get; set; }

    public decimal RemainingDays => EntitledDays + CarriedForwardDays - UsedDays;
}

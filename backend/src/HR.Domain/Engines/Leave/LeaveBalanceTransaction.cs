using HR.Domain.Common;

namespace HR.Domain.Engines.Leave;

/// <summary>Append-only ledger of every change to a leave balance (deduction on approval/assignment,
/// restoration on cancel, adjustment on edit). Delta is negative for a deduction, positive for a
/// restoration. Gives an auditable trail behind LeaveBalance.UsedDays.</summary>
public class LeaveBalanceTransaction : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public int Year { get; set; }

    public Guid? LeaveRecordId { get; set; }

    public decimal Delta { get; set; }          // -days (deduct) / +days (restore)
    public decimal BalanceAfter { get; set; }   // remaining balance after applying Delta

    public string? Reason { get; set; }
    public Guid? ActorUserId { get; set; }
    public DateTime At { get; set; }
}

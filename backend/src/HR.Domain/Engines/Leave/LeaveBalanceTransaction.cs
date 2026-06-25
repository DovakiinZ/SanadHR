using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Leave;

/// <summary>Append-only ledger of every change to a leave balance (accrual over service, deduction on
/// approval/assignment, restoration on cancel, adjustment/forfeiture). Delta is negative for a deduction,
/// positive for an accrual/restoration. Gives an auditable trail behind LeaveBalance.</summary>
public class LeaveBalanceTransaction : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public int Year { get; set; }

    /// <summary>What kind of movement this row represents. Defaults to Usage for backward compatibility.</summary>
    public LeaveTransactionType Type { get; set; } = LeaveTransactionType.Usage;

    public Guid? LeaveRecordId { get; set; }

    public decimal Delta { get; set; }          // -days (deduct/forfeit) / +days (accrue/restore)
    public decimal BalanceAfter { get; set; }   // remaining balance after applying Delta

    public string? Reason { get; set; }
    public Guid? ActorUserId { get; set; }
    public DateTime At { get; set; }
}

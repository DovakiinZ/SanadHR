using HR.Domain.Common;

namespace HR.Domain.Engines.Leave;

/// <summary>Audit-friendly record of a leave cancellation (who, when, why, how many days were restored).</summary>
public class LeaveCancellation : TenantEntity
{
    public Guid LeaveRecordId { get; set; }
    public string? Reason { get; set; }
    public decimal RestoredDays { get; set; }
    public Guid? CanceledByUserId { get; set; }
    public DateTime CanceledAt { get; set; }
}

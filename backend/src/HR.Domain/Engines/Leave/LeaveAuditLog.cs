using HR.Domain.Common;

namespace HR.Domain.Engines.Leave;

/// <summary>Append-only audit trail for leave records (Created / Assigned / Edited / Canceled / Printed).
/// Surfaced in the leave detail drawer.</summary>
public class LeaveAuditLog : TenantEntity
{
    public Guid? LeaveRecordId { get; set; }
    public Guid EmployeeId { get; set; }

    public string Action { get; set; } = null!;
    public string? DetailsAr { get; set; }
    public string? DetailsEn { get; set; }

    public Guid? ActorUserId { get; set; }
    public DateTime At { get; set; }
}

using HR.Domain.Common;

namespace HR.Domain.Engines.Requests;

/// <summary>
/// Declares where an approved request affects the platform. The Request engine reads this
/// to apply side effects deterministically (no hardcoded per-request logic).
/// </summary>
public class RequestImpactMapping : BaseEntity
{
    public Guid RequestTypeId { get; set; }

    public bool AffectsLeaveBalance { get; set; }
    public bool AffectsAttendance { get; set; }
    public bool AffectsPayroll { get; set; }
    public bool AffectsExpenses { get; set; }
    public bool AffectsLoans { get; set; }
    public bool CreatesLoanRecord { get; set; }
    public bool RequiresFinanceApproval { get; set; }

    public bool AffectsTimeline { get; set; } = true;
    public bool AffectsAudit { get; set; } = true;
    public bool NotifiesManager { get; set; } = true;
    public bool GeneratesDocument { get; set; }

    /// <summary>Extensible target config (JSONB) for future impact targets.</summary>
    public string? ExtraJson { get; set; }

    public RequestType RequestType { get; set; } = null!;
}

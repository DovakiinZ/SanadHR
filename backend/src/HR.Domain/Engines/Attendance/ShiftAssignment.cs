using HR.Domain.Common;

namespace HR.Domain.Engines.Attendance;

/// <summary>Assigns a <see cref="Shift"/> to a target (a specific employee, or everyone in a
/// department / branch / job-title) for an effective date range. Resolution is most-specific-wins:
/// Employee &gt; Department &gt; Branch &gt; JobTitle, then by <see cref="Priority"/> (higher first).</summary>
public class ShiftAssignment : TenantEntity
{
    public Guid ShiftId { get; set; }

    // Exactly one of these scopes is typically set (most-specific wins at resolution time).
    public Guid? EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? JobTitleId { get; set; }

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    /// <summary>Tie-breaker within the same scope specificity (higher wins).</summary>
    public int Priority { get; set; }

    public bool IsActive { get; set; } = true;

    public Shift Shift { get; set; } = null!;
}

using HR.Domain.Common;

namespace HR.Domain.Engines.Leave;

/// <summary>Header for a direct HR leave assignment (company holiday, forced/administrative leave, etc.).
/// One assignment can target many employees (directly, or by department/branch/job-title) and produces
/// one <see cref="LeaveRecord"/> per resolved employee.</summary>
public class LeaveAssignment : TenantEntity
{
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DaysCount { get; set; }

    /// <summary>"Employees" | "Department" | "Branch" | "JobTitle".</summary>
    public string TargetScope { get; set; } = "Employees";
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? JobTitleId { get; set; }

    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }

    public int AssignedCount { get; set; }
    public Guid? CreatedByUserId { get; set; }
}

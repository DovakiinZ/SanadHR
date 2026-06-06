using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Modules.Tasks.Entities;

public class HrTask : TenantEntity
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public HrTaskStatus Status { get; set; } = HrTaskStatus.NotStarted;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskSource Source { get; set; } = TaskSource.Manual;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid? AssignedById { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; } // JSON array stored as jsonb
    public int Progress { get; set; }
    public string? Notes { get; set; }

    public ICollection<HrTaskChecklist> Checklists { get; set; } = new List<HrTaskChecklist>();
    public ICollection<HrTaskComment> Comments { get; set; } = new List<HrTaskComment>();
    public ICollection<HrTaskActivity> Activities { get; set; } = new List<HrTaskActivity>();
}

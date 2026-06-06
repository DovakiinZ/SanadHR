using HR.Domain.Common;

namespace HR.Modules.Tasks.Entities;

public class HrTaskChecklist : BaseEntity
{
    public Guid TaskId { get; set; }
    public HrTask Task { get; set; } = null!;
    public string Title { get; set; } = null!;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}

using HR.Domain.Common;

namespace HR.Modules.Tasks.Entities;

public class HrTaskActivity : BaseEntity
{
    public Guid TaskId { get; set; }
    public HrTask Task { get; set; } = null!;
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = null!;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

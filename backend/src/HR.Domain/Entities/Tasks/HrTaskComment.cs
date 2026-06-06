using HR.Domain.Common;

namespace HR.Modules.Tasks.Entities;

public class HrTaskComment : BaseEntity
{
    public Guid TaskId { get; set; }
    public HrTask Task { get; set; } = null!;
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

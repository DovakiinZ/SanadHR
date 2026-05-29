namespace HR.Modules.Tasks.DTOs;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public string StatusAr { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public string PriorityAr { get; set; } = null!;
    public string Source { get; set; } = null!;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public Guid? AssignedById { get; set; }
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
    public int Progress { get; set; }
    public string? Notes { get; set; }
    public List<ChecklistItemDto> Checklists { get; set; } = new();
    public List<CommentDto> Comments { get; set; } = new();
    public List<ActivityDto> Activities { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class ChecklistItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class ActivityDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = null!;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}

using HR.Domain.Enums;
using HR.Modules.Tasks.DTOs;
using MediatR;

namespace HR.Modules.Tasks.Commands;

public record CreateTaskCommand : IRequest<TaskDto>
{
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public TaskPriority Priority { get; init; } = TaskPriority.Medium;
    public DateTime? DueDate { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? Category { get; init; }
    public List<string>? Tags { get; init; }
    public List<CreateChecklistItem>? Checklists { get; init; }
}

public class CreateChecklistItem
{
    public string Title { get; set; } = null!;
    public int SortOrder { get; set; }
}

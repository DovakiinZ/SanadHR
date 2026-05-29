using HR.Domain.Enums;
using HR.Modules.Tasks.DTOs;
using MediatR;

namespace HR.Modules.Tasks.Commands;

public record UpdateTaskCommand : IRequest<TaskDto>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public HrTaskStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? Category { get; init; }
    public List<string>? Tags { get; init; }
    public int Progress { get; init; }
    public string? Notes { get; init; }
}

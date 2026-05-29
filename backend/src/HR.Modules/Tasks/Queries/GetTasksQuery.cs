using HR.Application.Common.Models;
using HR.Modules.Tasks.DTOs;
using MediatR;

namespace HR.Modules.Tasks.Queries;

public record GetTasksQuery : IRequest<PaginatedList<TaskDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public string? Status { get; init; }
    public string? Priority { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? Category { get; init; }
}

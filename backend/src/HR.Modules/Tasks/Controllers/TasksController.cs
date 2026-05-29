using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Tasks.Commands;
using HR.Modules.Tasks.DTOs;
using HR.Modules.Tasks.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Tasks.Controllers;

[Authorize]
[Route("api/tasks")]
public class TasksController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Tasks.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<TaskDto>>>> GetAll(
        [FromQuery] GetTasksQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Tasks.View")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTaskByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("Tasks.Create")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> Create(
        [FromBody] CreateTaskCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Tasks.Edit")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> Update(
        Guid id, [FromBody] UpdateTaskCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Tasks.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteTaskCommand(id), ct);
        return OkResponse("Task deleted");
    }

    [HttpPost("{id:guid}/comments")]
    [RequirePermission("Tasks.Edit")]
    public async Task<ActionResult<ApiResponse<CommentDto>>> AddComment(
        Guid id, [FromBody] AddCommentRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new AddTaskCommentCommand(id, request.Content), ct);
        return CreatedResponse(result);
    }
}

public class AddCommentRequest
{
    public string Content { get; set; } = null!;
}

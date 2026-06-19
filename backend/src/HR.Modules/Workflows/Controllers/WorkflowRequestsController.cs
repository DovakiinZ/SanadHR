using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Workflows.Commands;
using HR.Modules.Workflows.DTOs;
using HR.Modules.Workflows.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Workflows.Controllers;

/// <summary>
/// The run-time side: starting requests against a definition, executing the current step
/// (approve / reject), cancelling, and reading request state + audit trail.
/// </summary>
[Authorize]
[Route("api/workflow-requests")]
public class WorkflowRequestsController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Workflows.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<WorkflowRequestDto>>>> GetAll(
        [FromQuery] GetWorkflowRequestsQuery query, CancellationToken ct)
        => OkResponse(await Mediator.Send(query, ct));

    [HttpGet("pending")]
    [RequirePermission("Workflows.View")]
    public async Task<ActionResult<ApiResponse<List<WorkflowRequestDto>>>> Pending(CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetPendingWorkflowApprovalsQuery(), ct));

    [HttpGet("{id:guid}")]
    [RequirePermission("Workflows.View")]
    public async Task<ActionResult<ApiResponse<WorkflowRequestDto>>> GetById(Guid id, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetWorkflowRequestByIdQuery(id), ct));

    /// <summary>Start a new request against a (published, active) definition.</summary>
    [HttpPost]
    [RequirePermission("Workflows.View")]
    public async Task<ActionResult<ApiResponse<WorkflowRequestDto>>> Start(
        [FromBody] StartWorkflowRequestCommand command, CancellationToken ct)
        => CreatedResponse(await Mediator.Send(command, ct));

    /// <summary>Execute the current step — approve or reject — and let the engine advance.</summary>
    [HttpPost("{id:guid}/execute")]
    [RequirePermission("Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowRequestDto>>> Execute(
        Guid id, [FromBody] ExecuteWorkflowStepCommand command, CancellationToken ct)
        => OkResponse(await Mediator.Send(command with { RequestId = id }, ct));

    [HttpPost("{id:guid}/cancel")]
    [RequirePermission("Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowRequestDto>>> Cancel(
        Guid id, [FromBody] CancelWorkflowRequestCommand command, CancellationToken ct)
        => OkResponse(await Mediator.Send(command with { RequestId = id }, ct));
}

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
/// CRUD + graph editing for workflow definitions (the design-time side that the visual builder talks to).
/// </summary>
[Authorize]
[Route("api/workflow-definitions")]
public class WorkflowDefinitionsController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Workflows.View")]
    public async Task<ActionResult<ApiResponse<List<WorkflowDefinitionSummaryDto>>>> GetAll(
        [FromQuery] GetWorkflowDefinitionsQuery query, CancellationToken ct)
        => OkResponse(await Mediator.Send(query, ct));

    [HttpGet("{id:guid}")]
    [RequirePermission("Workflows.View")]
    public async Task<ActionResult<ApiResponse<WorkflowDefinitionDto>>> GetById(Guid id, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetWorkflowDefinitionByIdQuery(id), ct));

    [HttpPost]
    [RequirePermission("Workflows.Create")]
    public async Task<ActionResult<ApiResponse<WorkflowDefinitionDto>>> Create(
        [FromBody] CreateWorkflowDefinitionCommand command, CancellationToken ct)
        => CreatedResponse(await Mediator.Send(command, ct));

    /// <summary>Save the whole definition incl. its step graph. Rejected (400) if the graph is invalid.</summary>
    [HttpPut("{id:guid}")]
    [RequirePermission("Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowDefinitionDto>>> Update(
        Guid id, [FromBody] UpdateWorkflowDefinitionCommand command, CancellationToken ct)
        => OkResponse(await Mediator.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")]
    [RequirePermission("Workflows.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWorkflowDefinitionCommand(id), ct);
        return OkResponse("Workflow deleted.");
    }
}

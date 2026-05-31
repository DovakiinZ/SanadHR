using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.Workflows;
using HR.Modules.Platform.DTOs.Workflows;
using HR.Modules.Platform.Queries.Workflows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/workflow-enhancements")]
public class WorkflowEnhancementsController : BaseApiController
{
    // === Dynamic Approvers ===

    [HttpGet("nodes/{nodeId:guid}/dynamic-approvers")]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<List<WorkflowDynamicApproverDto>>>> GetDynamicApprovers(
        Guid nodeId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetDynamicApproversByNodeQuery(nodeId), ct);
        return OkResponse(result);
    }

    [HttpPost("nodes/{nodeId:guid}/dynamic-approvers")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowDynamicApproverDto>>> AddDynamicApprover(
        Guid nodeId, [FromBody] AddDynamicApproverCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { WorkflowNodeId = nodeId }, ct);
        return CreatedResponse(result);
    }

    [HttpPut("dynamic-approvers/{id:guid}")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowDynamicApproverDto>>> UpdateDynamicApprover(
        Guid id, [FromBody] UpdateDynamicApproverCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("dynamic-approvers/{id:guid}")]
    [RequirePermission("Platform.Workflows.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteDynamicApprover(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteDynamicApproverCommand(id), ct);
        return OkResponse("Dynamic approver deleted");
    }

    // === Dynamic Conditions ===

    [HttpGet("nodes/{nodeId:guid}/dynamic-conditions")]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<List<WorkflowDynamicConditionDto>>>> GetDynamicConditions(
        Guid nodeId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetDynamicConditionsByNodeQuery(nodeId), ct);
        return OkResponse(result);
    }

    [HttpPost("nodes/{nodeId:guid}/dynamic-conditions")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowDynamicConditionDto>>> AddDynamicCondition(
        Guid nodeId, [FromBody] AddDynamicConditionCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { WorkflowNodeId = nodeId }, ct);
        return CreatedResponse(result);
    }

    [HttpPut("dynamic-conditions/{id:guid}")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowDynamicConditionDto>>> UpdateDynamicCondition(
        Guid id, [FromBody] UpdateDynamicConditionCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("dynamic-conditions/{id:guid}")]
    [RequirePermission("Platform.Workflows.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteDynamicCondition(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteDynamicConditionCommand(id), ct);
        return OkResponse("Dynamic condition deleted");
    }

    // === Workflow Actions ===

    [HttpGet("nodes/{nodeId:guid}/actions")]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<List<WorkflowActionDto>>>> GetWorkflowActions(
        Guid nodeId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetWorkflowActionsByNodeQuery(nodeId), ct);
        return OkResponse(result);
    }

    [HttpPost("nodes/{nodeId:guid}/actions")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowActionDto>>> AddWorkflowAction(
        Guid nodeId, [FromBody] AddWorkflowActionCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { WorkflowNodeId = nodeId }, ct);
        return CreatedResponse(result);
    }

    [HttpPut("actions/{id:guid}")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowActionDto>>> UpdateWorkflowAction(
        Guid id, [FromBody] UpdateWorkflowActionCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("actions/{id:guid}")]
    [RequirePermission("Platform.Workflows.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteWorkflowAction(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWorkflowActionCommand(id), ct);
        return OkResponse("Workflow action deleted");
    }

    // === Simulations ===

    [HttpGet("versions/{versionId:guid}/simulations")]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<List<WorkflowSimulationDto>>>> GetSimulations(
        Guid versionId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetWorkflowSimulationsQuery(versionId), ct);
        return OkResponse(result);
    }

    [HttpGet("simulations/{id:guid}")]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<WorkflowSimulationDto>>> GetSimulationById(
        Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetWorkflowSimulationByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPost("versions/{versionId:guid}/simulate")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowSimulationDto>>> RunSimulation(
        Guid versionId, [FromBody] RunWorkflowSimulationCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { WorkflowVersionId = versionId }, ct);
        return CreatedResponse(result);
    }

    [HttpDelete("simulations/{id:guid}")]
    [RequirePermission("Platform.Workflows.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteSimulation(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWorkflowSimulationCommand(id), ct);
        return OkResponse("Simulation deleted");
    }
}

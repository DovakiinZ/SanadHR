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
[Route("api/platform/workflows")]
public class WorkflowsController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<WorkflowDefinitionDto>>>> GetAll(
        [FromQuery] GetWorkflowDefinitionsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("Platform.Workflows.Create")]
    public async Task<ActionResult<ApiResponse<WorkflowDefinitionDto>>> Create(
        [FromBody] CreateWorkflowDefinitionCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowDefinitionDto>>> Update(
        Guid id, [FromBody] UpdateWorkflowDefinitionCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Platform.Workflows.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWorkflowDefinitionCommand(id), ct);
        return OkResponse("Workflow definition deleted");
    }

    // Version management
    [HttpPost("{id:guid}/versions")]
    [RequirePermission("Platform.Workflows.Create")]
    public async Task<ActionResult<ApiResponse<WorkflowVersionDto>>> CreateVersion(
        Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateWorkflowVersionCommand { WorkflowDefinitionId = id }, ct);
        return CreatedResponse(result);
    }

    [HttpPut("versions/{versionId:guid}/publish")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowVersionDto>>> PublishVersion(Guid versionId, CancellationToken ct)
    {
        var result = await Mediator.Send(new PublishWorkflowVersionCommand(versionId), ct);
        return OkResponse(result);
    }

    [HttpGet("versions/{versionId:guid}")]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<WorkflowVersionDetailDto>>> GetVersion(Guid versionId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetWorkflowVersionQuery(versionId), ct);
        return OkResponse(result);
    }

    // Node management
    [HttpPost("versions/{versionId:guid}/nodes")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowNodeDto>>> AddNode(
        Guid versionId, [FromBody] AddWorkflowNodeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { WorkflowVersionId = versionId }, ct);
        return CreatedResponse(result);
    }

    [HttpPut("nodes/{nodeId:guid}")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowNodeDto>>> UpdateNode(
        Guid nodeId, [FromBody] UpdateWorkflowNodeCommand command, CancellationToken ct)
    {
        if (nodeId != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("nodes/{nodeId:guid}")]
    [RequirePermission("Platform.Workflows.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteNode(Guid nodeId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWorkflowNodeCommand(nodeId), ct);
        return OkResponse("Node deleted");
    }

    // Edge management
    [HttpPost("versions/{versionId:guid}/edges")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowEdgeDto>>> AddEdge(
        Guid versionId, [FromBody] AddWorkflowEdgeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { WorkflowVersionId = versionId }, ct);
        return CreatedResponse(result);
    }

    [HttpPut("edges/{edgeId:guid}")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowEdgeDto>>> UpdateEdge(
        Guid edgeId, [FromBody] UpdateWorkflowEdgeCommand command, CancellationToken ct)
    {
        if (edgeId != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("edges/{edgeId:guid}")]
    [RequirePermission("Platform.Workflows.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteEdge(Guid edgeId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWorkflowEdgeCommand(edgeId), ct);
        return OkResponse("Edge deleted");
    }

    // Condition management
    [HttpPost("nodes/{nodeId:guid}/conditions")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowConditionDto>>> AddCondition(
        Guid nodeId, [FromBody] AddWorkflowConditionCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { WorkflowNodeId = nodeId }, ct);
        return CreatedResponse(result);
    }

    [HttpDelete("conditions/{conditionId:guid}")]
    [RequirePermission("Platform.Workflows.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteCondition(Guid conditionId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWorkflowConditionCommand(conditionId), ct);
        return OkResponse("Condition deleted");
    }

    // Approver rule management
    [HttpPost("nodes/{nodeId:guid}/approver-rules")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<WorkflowApproverRuleDto>>> SetApproverRule(
        Guid nodeId, [FromBody] SetWorkflowApproverRuleCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { WorkflowNodeId = nodeId }, ct);
        return CreatedResponse(result);
    }

    [HttpDelete("approver-rules/{ruleId:guid}")]
    [RequirePermission("Platform.Workflows.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteApproverRule(Guid ruleId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWorkflowApproverRuleCommand(ruleId), ct);
        return OkResponse("Approver rule deleted");
    }

    // Instance management
    [HttpPost("start")]
    [RequirePermission("Platform.Workflows.Create")]
    public async Task<ActionResult<ApiResponse<WorkflowInstanceDto>>> Start(
        [FromBody] StartWorkflowCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPost("steps/{stepId:guid}/process")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse>> ProcessStep(
        Guid stepId, [FromBody] ProcessWorkflowStepCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return OkResponse("Step processed");
    }

    [HttpGet("instances")]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<WorkflowInstanceDto>>>> GetInstances(
        [FromQuery] GetWorkflowInstancesQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpGet("instances/{instanceId:guid}")]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<WorkflowInstanceDto>>> GetInstanceById(Guid instanceId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetWorkflowInstanceByIdQuery(instanceId), ct);
        return OkResponse(result);
    }

    [HttpPost("instances/{instanceId:guid}/cancel")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse>> CancelInstance(Guid instanceId, CancellationToken ct)
    {
        await Mediator.Send(new CancelWorkflowInstanceCommand(instanceId), ct);
        return OkResponse("Workflow instance cancelled");
    }

    [HttpGet("pending-approvals/{userId:guid}")]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<List<WorkflowInstanceStepDto>>>> GetPendingApprovals(
        Guid userId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPendingApprovalsQuery(userId), ct);
        return OkResponse(result);
    }
}

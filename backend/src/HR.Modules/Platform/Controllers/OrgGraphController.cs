using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.OrgGraph;
using HR.Modules.Platform.DTOs.OrgGraph;
using HR.Modules.Platform.Queries.OrgGraph;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/org-graph")]
public class OrgGraphController : BaseApiController
{
    // Nodes

    [HttpGet("tree")]
    [RequirePermission("Platform.OrgGraph.View")]
    public async Task<ActionResult<ApiResponse<OrgGraphTreeDto>>> GetOrgGraphTree(
        [FromQuery] GetOrgGraphTreeQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpGet("nodes/{id:guid}")]
    [RequirePermission("Platform.OrgGraph.View")]
    public async Task<ActionResult<ApiResponse<OrgNodeDto>>> GetNodeById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOrgNodeByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPost("nodes")]
    [RequirePermission("Platform.OrgGraph.Create")]
    public async Task<ActionResult<ApiResponse<OrgNodeDto>>> CreateNode(
        [FromBody] CreateOrgNodeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("nodes/{id:guid}")]
    [RequirePermission("Platform.OrgGraph.Edit")]
    public async Task<ActionResult<ApiResponse<OrgNodeDto>>> UpdateNode(
        Guid id, [FromBody] UpdateOrgNodeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { Id = id }, ct);
        return OkResponse(result);
    }

    [HttpDelete("nodes/{id:guid}")]
    [RequirePermission("Platform.OrgGraph.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteNode(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteOrgNodeCommand(id), ct);
        return OkResponse("Node deleted");
    }

    [HttpPut("nodes/{id:guid}/move")]
    [RequirePermission("Platform.OrgGraph.Edit")]
    public async Task<ActionResult<ApiResponse<OrgNodeDto>>> MoveNode(
        Guid id, [FromBody] MoveOrgNodeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { Id = id }, ct);
        return OkResponse(result);
    }

    [HttpPut("nodes/positions")]
    [RequirePermission("Platform.OrgGraph.Edit")]
    public async Task<ActionResult<ApiResponse>> BulkUpdatePositions(
        [FromBody] BulkUpdateNodePositionsCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return OkResponse("Positions updated");
    }

    // Edges

    [HttpPost("edges")]
    [RequirePermission("Platform.OrgGraph.Create")]
    public async Task<ActionResult<ApiResponse<OrgEdgeDto>>> CreateEdge(
        [FromBody] CreateOrgEdgeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpDelete("edges/{id:guid}")]
    [RequirePermission("Platform.OrgGraph.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteEdge(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteOrgEdgeCommand(id), ct);
        return OkResponse("Edge deleted");
    }

    // Layouts

    [HttpGet("layouts")]
    [RequirePermission("Platform.OrgGraph.View")]
    public async Task<ActionResult<ApiResponse<List<OrgGraphLayoutDto>>>> GetLayouts(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOrgGraphLayoutsQuery(), ct);
        return OkResponse(result);
    }

    [HttpPost("layouts")]
    [RequirePermission("Platform.OrgGraph.Create")]
    public async Task<ActionResult<ApiResponse<OrgGraphLayoutDto>>> CreateLayout(
        [FromBody] CreateOrgGraphLayoutCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("layouts/{id:guid}")]
    [RequirePermission("Platform.OrgGraph.Edit")]
    public async Task<ActionResult<ApiResponse<OrgGraphLayoutDto>>> UpdateLayout(
        Guid id, [FromBody] UpdateOrgGraphLayoutCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { Id = id }, ct);
        return OkResponse(result);
    }

    [HttpDelete("layouts/{id:guid}")]
    [RequirePermission("Platform.OrgGraph.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteLayout(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteOrgGraphLayoutCommand(id), ct);
        return OkResponse("Layout deleted");
    }

    // Reporting Lines

    [HttpGet("reporting-lines")]
    [RequirePermission("Platform.OrgGraph.View")]
    public async Task<ActionResult<ApiResponse<List<EmployeeReportingLineDto>>>> GetReportingLines(
        [FromQuery] GetEmployeeReportingLinesQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpGet("reporting-lines/subordinates/{managerId:guid}")]
    [RequirePermission("Platform.OrgGraph.View")]
    public async Task<ActionResult<ApiResponse<List<EmployeeReportingLineDto>>>> GetSubordinates(
        Guid managerId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetSubordinatesQuery(managerId), ct);
        return OkResponse(result);
    }

    [HttpPost("reporting-lines")]
    [RequirePermission("Platform.OrgGraph.Create")]
    public async Task<ActionResult<ApiResponse<EmployeeReportingLineDto>>> CreateReportingLine(
        [FromBody] CreateEmployeeReportingLineCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("reporting-lines/{id:guid}")]
    [RequirePermission("Platform.OrgGraph.Edit")]
    public async Task<ActionResult<ApiResponse<EmployeeReportingLineDto>>> UpdateReportingLine(
        Guid id, [FromBody] UpdateEmployeeReportingLineCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { Id = id }, ct);
        return OkResponse(result);
    }

    [HttpDelete("reporting-lines/{id:guid}")]
    [RequirePermission("Platform.OrgGraph.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteReportingLine(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteEmployeeReportingLineCommand(id), ct);
        return OkResponse("Reporting line deleted");
    }
}

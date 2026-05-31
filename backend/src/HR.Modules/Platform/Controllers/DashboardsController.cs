using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.Dashboards;
using HR.Modules.Platform.DTOs.Dashboards;
using HR.Modules.Platform.Queries.Dashboards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/dashboards")]
public class DashboardsController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Platform.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<DashboardDefinitionDto>>>> GetAll(
        [FromQuery] GetDashboardDefinitionsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Platform.View")]
    public async Task<ActionResult<ApiResponse<DashboardDefinitionDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetDashboardDefinitionByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("Platform.Create")]
    public async Task<ActionResult<ApiResponse<DashboardDefinitionDto>>> Create(
        [FromBody] CreateDashboardDefinitionCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Platform.Edit")]
    public async Task<ActionResult<ApiResponse<DashboardDefinitionDto>>> Update(
        Guid id, [FromBody] UpdateDashboardDefinitionCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Platform.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteDashboardDefinitionCommand(id), ct);
        return OkResponse("Dashboard deleted");
    }
}

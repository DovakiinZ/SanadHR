using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.Automation;
using HR.Modules.Platform.DTOs.Automation;
using HR.Modules.Platform.Queries.Automation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/automation-rules")]
public class AutomationController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Platform.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<AutomationRuleDto>>>> GetAll(
        [FromQuery] GetAutomationRulesQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Platform.View")]
    public async Task<ActionResult<ApiResponse<AutomationRuleDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAutomationRuleByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("Platform.Create")]
    public async Task<ActionResult<ApiResponse<AutomationRuleDto>>> Create(
        [FromBody] CreateAutomationRuleCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Platform.Edit")]
    public async Task<ActionResult<ApiResponse<AutomationRuleDto>>> Update(
        Guid id, [FromBody] UpdateAutomationRuleCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Platform.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteAutomationRuleCommand(id), ct);
        return OkResponse("Automation rule deleted");
    }
}

using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.Permissions;
using HR.Modules.Platform.DTOs.Permissions;
using HR.Modules.Platform.Queries.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/permission-templates")]
public class PermissionsController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Platform.Permissions.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<PermissionTemplateDto>>>> GetAll(
        [FromQuery] GetPermissionTemplatesQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Platform.Permissions.View")]
    public async Task<ActionResult<ApiResponse<PermissionTemplateDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPermissionTemplateByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("Platform.Permissions.Create")]
    public async Task<ActionResult<ApiResponse<PermissionTemplateDto>>> Create(
        [FromBody] CreatePermissionTemplateCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Platform.Permissions.Edit")]
    public async Task<ActionResult<ApiResponse<PermissionTemplateDto>>> Update(
        Guid id, [FromBody] UpdatePermissionTemplateCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Platform.Permissions.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeletePermissionTemplateCommand(id), ct);
        return OkResponse("Permission template deleted");
    }

    // Template item management
    [HttpPost("{id:guid}/items")]
    [RequirePermission("Platform.Permissions.Edit")]
    public async Task<ActionResult<ApiResponse<PermissionTemplateItemDto>>> AddItem(
        Guid id, [FromBody] AddPermissionTemplateItemCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { PermissionTemplateId = id }, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}/items/{itemId:guid}")]
    [RequirePermission("Platform.Permissions.Edit")]
    public async Task<ActionResult<ApiResponse<PermissionTemplateItemDto>>> UpdateItem(
        Guid id, Guid itemId, [FromBody] UpdatePermissionTemplateItemCommand command, CancellationToken ct)
    {
        if (itemId != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [RequirePermission("Platform.Permissions.Delete")]
    public async Task<ActionResult<ApiResponse>> RemoveItem(Guid id, Guid itemId, CancellationToken ct)
    {
        await Mediator.Send(new RemovePermissionTemplateItemCommand(itemId), ct);
        return OkResponse("Item removed");
    }

    // Template assignment
    [HttpPost("assign")]
    [RequirePermission("Platform.Permissions.Edit")]
    public async Task<ActionResult<ApiResponse<UserPermissionTemplateDto>>> AssignTemplate(
        [FromBody] AssignPermissionTemplateCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpDelete("assign/{userId:guid}/{templateId:guid}")]
    [RequirePermission("Platform.Permissions.Edit")]
    public async Task<ActionResult<ApiResponse>> RevokeTemplate(Guid userId, Guid templateId, CancellationToken ct)
    {
        await Mediator.Send(new RevokePermissionTemplateCommand(userId, templateId), ct);
        return OkResponse("Template revoked");
    }

    // User effective permissions
    [HttpGet("~/api/platform/permissions/user/{userId:guid}/effective")]
    [RequirePermission("Platform.Permissions.View")]
    public async Task<ActionResult<ApiResponse<List<UserEffectivePermissionDto>>>> GetEffectivePermissions(
        Guid userId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetUserEffectivePermissionsQuery(userId), ct);
        return OkResponse(result);
    }

    [HttpGet("~/api/platform/permissions/available")]
    [RequirePermission("Platform.Permissions.View")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetAvailablePermissions(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAvailablePermissionsQuery(), ct);
        return OkResponse(result);
    }

    // User permission overrides
    [HttpPost("~/api/platform/permissions/user/{userId:guid}/overrides")]
    [RequirePermission("Platform.Permissions.Edit")]
    public async Task<ActionResult<ApiResponse<Guid>>> SetOverride(
        Guid userId, [FromBody] SetUserPermissionOverrideCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { UserId = userId }, ct);
        return CreatedResponse(result);
    }

    [HttpDelete("~/api/platform/permissions/user/{userId:guid}/overrides/{id:guid}")]
    [RequirePermission("Platform.Permissions.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteOverride(Guid userId, Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteUserPermissionOverrideCommand(id), ct);
        return OkResponse("Override deleted");
    }

    // User permission scopes
    [HttpPost("~/api/platform/permissions/user/{userId:guid}/scopes")]
    [RequirePermission("Platform.Permissions.Edit")]
    public async Task<ActionResult<ApiResponse<Guid>>> SetScope(
        Guid userId, [FromBody] SetUserPermissionScopeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { UserId = userId }, ct);
        return CreatedResponse(result);
    }

    [HttpDelete("~/api/platform/permissions/user/{userId:guid}/scopes/{id:guid}")]
    [RequirePermission("Platform.Permissions.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteScope(Guid userId, Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteUserPermissionScopeCommand(id), ct);
        return OkResponse("Scope deleted");
    }
}

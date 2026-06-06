using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.MasterData;
using HR.Modules.Platform.DTOs.MasterData;
using HR.Modules.Platform.Queries.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

/// <summary>
/// Tenant master data management. Backs the (future) /settings/master-data admin UI.
/// All structured business objects (job titles, leave types, allowance types,
/// document types, tags, …) are managed here and consumed via /api/lookups.
/// </summary>
[Authorize]
[Route("api/platform/master-data")]
public class MasterDataController : BaseApiController
{
    // ─── Reads ───────────────────────────────────────────────────────────────

    /// <summary>List the catalogue of object types with live item counts.</summary>
    [HttpGet("types")]
    [RequirePermission("Platform.MasterData.View")]
    public async Task<ActionResult<ApiResponse<List<MasterDataObjectTypeDto>>>> GetTypes(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMasterDataObjectTypesQuery(), ct);
        return OkResponse(result);
    }

    /// <summary>List items for an object type (management view, includes inactive when requested).</summary>
    [HttpGet]
    [RequirePermission("Platform.MasterData.View")]
    public async Task<ActionResult<ApiResponse<List<MasterDataItemDto>>>> GetItems(
        [FromQuery] string objectType,
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetMasterDataItemsQuery
        {
            ObjectType = objectType,
            Search = search,
            IncludeInactive = includeInactive
        }, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Platform.MasterData.View")]
    public async Task<ActionResult<ApiResponse<MasterDataItemDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMasterDataItemByIdQuery(id), ct);
        return OkResponse(result);
    }

    /// <summary>Where-used report for an item (used before deactivate/delete/merge).</summary>
    [HttpGet("{id:guid}/usage")]
    [RequirePermission("Platform.MasterData.View")]
    public async Task<ActionResult<ApiResponse<MasterDataUsageDto>>> GetUsage(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMasterDataUsageQuery(id), ct);
        return OkResponse(result);
    }

    // ─── Writes ──────────────────────────────────────────────────────────────

    [HttpPost]
    [RequirePermission("Platform.MasterData.Create")]
    public async Task<ActionResult<ApiResponse<MasterDataItemDto>>> Create(
        [FromBody] CreateMasterDataItemCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    /// <summary>Seed this tenant's default master data and register object types in the registry (idempotent).</summary>
    [HttpPost("seed-defaults")]
    [RequirePermission("Platform.MasterData.Create")]
    public async Task<ActionResult<ApiResponse<SeedMasterDataResultDto>>> SeedDefaults(CancellationToken ct)
    {
        var result = await Mediator.Send(new SeedDefaultMasterDataCommand(), ct);
        return OkResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Platform.MasterData.Edit")]
    public async Task<ActionResult<ApiResponse<MasterDataItemDto>>> Update(
        Guid id, [FromBody] UpdateMasterDataItemCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [RequirePermission("Platform.MasterData.Edit")]
    public async Task<ActionResult<ApiResponse<MasterDataItemDto>>> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateMasterDataItemCommand(id), ct);
        return OkResponse(result);
    }

    [HttpPost("reorder")]
    [RequirePermission("Platform.MasterData.Edit")]
    public async Task<ActionResult<ApiResponse>> Reorder(
        [FromBody] ReorderMasterDataItemsCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return OkResponse("Reordered");
    }

    [HttpPost("merge")]
    [RequirePermission("Platform.MasterData.Edit")]
    public async Task<ActionResult<ApiResponse<MasterDataItemDto>>> Merge(
        [FromBody] MergeMasterDataItemsCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Platform.MasterData.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteMasterDataItemCommand(id), ct);
        return OkResponse("Master data item deleted");
    }
}

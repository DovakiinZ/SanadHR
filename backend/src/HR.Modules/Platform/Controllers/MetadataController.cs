using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.Metadata;
using HR.Modules.Platform.DTOs.Metadata;
using HR.Modules.Platform.Queries.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/metadata-definitions")]
public class MetadataController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Platform.Metadata.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<MetadataDefinitionDto>>>> GetAll(
        [FromQuery] GetMetadataDefinitionsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Platform.Metadata.View")]
    public async Task<ActionResult<ApiResponse<MetadataDefinitionDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMetadataDefinitionByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("Platform.Metadata.Create")]
    public async Task<ActionResult<ApiResponse<MetadataDefinitionDto>>> Create(
        [FromBody] CreateMetadataDefinitionCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Platform.Metadata.Edit")]
    public async Task<ActionResult<ApiResponse<MetadataDefinitionDto>>> Update(
        Guid id, [FromBody] UpdateMetadataDefinitionCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Platform.Metadata.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteMetadataDefinitionCommand(id), ct);
        return OkResponse("Metadata definition deleted");
    }

    // Field endpoints
    [HttpPost("{id:guid}/fields")]
    [RequirePermission("Platform.Metadata.Edit")]
    public async Task<ActionResult<ApiResponse<MetadataFieldDto>>> AddField(
        Guid id, [FromBody] AddMetadataFieldCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { MetadataDefinitionId = id }, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}/fields/{fieldId:guid}")]
    [RequirePermission("Platform.Metadata.Edit")]
    public async Task<ActionResult<ApiResponse<MetadataFieldDto>>> UpdateField(
        Guid id, Guid fieldId, [FromBody] UpdateMetadataFieldCommand command, CancellationToken ct)
    {
        if (fieldId != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}/fields/{fieldId:guid}")]
    [RequirePermission("Platform.Metadata.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteField(Guid id, Guid fieldId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteMetadataFieldCommand(fieldId), ct);
        return OkResponse("Field deleted");
    }

    [HttpPut("{id:guid}/fields/reorder")]
    [RequirePermission("Platform.Metadata.Edit")]
    public async Task<ActionResult<ApiResponse>> ReorderFields(
        Guid id, [FromBody] ReorderMetadataFieldsCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { MetadataDefinitionId = id }, ct);
        return OkResponse("Fields reordered");
    }

    // Option endpoints
    [HttpPost("{id:guid}/fields/{fieldId:guid}/options")]
    [RequirePermission("Platform.Metadata.Edit")]
    public async Task<ActionResult<ApiResponse<MetadataOptionDto>>> AddOption(
        Guid id, Guid fieldId, [FromBody] AddMetadataOptionCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { MetadataFieldId = fieldId }, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}/fields/{fieldId:guid}/options/{optionId:guid}")]
    [RequirePermission("Platform.Metadata.Edit")]
    public async Task<ActionResult<ApiResponse<MetadataOptionDto>>> UpdateOption(
        Guid id, Guid fieldId, Guid optionId, [FromBody] UpdateMetadataOptionCommand command, CancellationToken ct)
    {
        if (optionId != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}/fields/{fieldId:guid}/options/{optionId:guid}")]
    [RequirePermission("Platform.Metadata.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteOption(
        Guid id, Guid fieldId, Guid optionId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteMetadataOptionCommand(optionId), ct);
        return OkResponse("Option deleted");
    }

    // Metadata values
    [HttpGet("~/api/platform/metadata-values/{entityType}/{entityId:guid}")]
    [RequirePermission("Platform.Metadata.View")]
    public async Task<ActionResult<ApiResponse<List<MetadataValueDto>>>> GetValues(
        string entityType, Guid entityId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMetadataValuesQuery(entityType, entityId), ct);
        return OkResponse(result);
    }

    [HttpPost("~/api/platform/metadata-values/{entityType}/{entityId:guid}")]
    [RequirePermission("Platform.Metadata.Edit")]
    public async Task<ActionResult<ApiResponse<MetadataValueDto>>> SaveValue(
        string entityType, Guid entityId, [FromBody] SaveMetadataValueCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }
}

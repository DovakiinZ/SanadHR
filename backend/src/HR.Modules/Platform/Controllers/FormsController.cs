using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.Forms;
using HR.Modules.Platform.DTOs.Forms;
using HR.Modules.Platform.Queries.Forms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/forms")]
public class FormsController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Platform.Forms.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<FormDefinitionDto>>>> GetAll(
        [FromQuery] GetFormDefinitionsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Platform.Forms.View")]
    public async Task<ActionResult<ApiResponse<FormDefinitionDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetFormDefinitionByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("Platform.Forms.Create")]
    public async Task<ActionResult<ApiResponse<FormDefinitionDto>>> Create(
        [FromBody] CreateFormDefinitionCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Platform.Forms.Edit")]
    public async Task<ActionResult<ApiResponse<FormDefinitionDto>>> Update(
        Guid id, [FromBody] UpdateFormDefinitionCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Platform.Forms.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteFormDefinitionCommand(id), ct);
        return OkResponse("Form definition deleted");
    }

    // Field management
    [HttpPost("{id:guid}/fields")]
    [RequirePermission("Platform.Forms.Edit")]
    public async Task<ActionResult<ApiResponse<FormFieldDto>>> AddField(
        Guid id, [FromBody] AddFormFieldCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { FormDefinitionId = id }, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}/fields/{fieldId:guid}")]
    [RequirePermission("Platform.Forms.Edit")]
    public async Task<ActionResult<ApiResponse<FormFieldDto>>> UpdateField(
        Guid id, Guid fieldId, [FromBody] UpdateFormFieldCommand command, CancellationToken ct)
    {
        if (fieldId != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}/fields/{fieldId:guid}")]
    [RequirePermission("Platform.Forms.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteField(Guid id, Guid fieldId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteFormFieldCommand(fieldId), ct);
        return OkResponse("Field deleted");
    }

    [HttpPut("{id:guid}/fields/reorder")]
    [RequirePermission("Platform.Forms.Edit")]
    public async Task<ActionResult<ApiResponse>> ReorderFields(
        Guid id, [FromBody] ReorderFormFieldsCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { FormDefinitionId = id }, ct);
        return OkResponse("Fields reordered");
    }

    // Publish/Unpublish
    [HttpPost("{id:guid}/publish")]
    [RequirePermission("Platform.Forms.Edit")]
    public async Task<ActionResult<ApiResponse<FormDefinitionDto>>> Publish(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new PublishFormCommand(id), ct);
        return OkResponse(result);
    }

    [HttpPost("{id:guid}/unpublish")]
    [RequirePermission("Platform.Forms.Edit")]
    public async Task<ActionResult<ApiResponse<FormDefinitionDto>>> Unpublish(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new UnpublishFormCommand(id), ct);
        return OkResponse(result);
    }

    // Clone
    [HttpPost("{id:guid}/clone")]
    [RequirePermission("Platform.Forms.Create")]
    public async Task<ActionResult<ApiResponse<FormDefinitionDto>>> Clone(
        Guid id, [FromBody] CloneFormCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { SourceFormId = id }, ct);
        return CreatedResponse(result);
    }

    // Submit
    [HttpPost("{id:guid}/submit")]
    [RequirePermission("Platform.Forms.Create")]
    public async Task<ActionResult<ApiResponse<FormSubmissionDto>>> Submit(
        Guid id, [FromBody] SubmitFormCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    // Submissions
    [HttpGet("~/api/platform/form-submissions")]
    [RequirePermission("Platform.Forms.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<FormSubmissionDto>>>> GetSubmissions(
        [FromQuery] GetFormSubmissionsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpGet("~/api/platform/form-submissions/{id:guid}")]
    [RequirePermission("Platform.Forms.View")]
    public async Task<ActionResult<ApiResponse<FormSubmissionDto>>> GetSubmissionById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetFormSubmissionByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPut("~/api/platform/form-submissions/{id:guid}/status")]
    [RequirePermission("Platform.Forms.Edit")]
    public async Task<ActionResult<ApiResponse<FormSubmissionDto>>> UpdateSubmissionStatus(
        Guid id, [FromBody] UpdateFormSubmissionStatusCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }
}

using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.Documents;
using HR.Modules.Platform.DTOs.Documents;
using HR.Modules.Platform.Queries.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

/// <summary>Page templates (document chrome): header/footer/margins/watermark presets that
/// document templates inherit. System presets ship out of the box and cannot be deleted.</summary>
[Authorize]
[Route("api/platform/page-templates")]
public class PageTemplatesController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Platform.Documents.View")]
    public async Task<ActionResult<ApiResponse<List<PageTemplateDto>>>> GetAll(CancellationToken ct)
    { var result = await Mediator.Send(new GetPageTemplatesQuery(), ct); return OkResponse(result); }

    [HttpGet("{id:guid}")]
    [RequirePermission("Platform.Documents.View")]
    public async Task<ActionResult<ApiResponse<PageTemplateDto>>> GetById(Guid id, CancellationToken ct)
    { var result = await Mediator.Send(new GetPageTemplateByIdQuery(id), ct); return OkResponse(result); }

    [HttpPost]
    [RequirePermission("Platform.Documents.Create")]
    public async Task<ActionResult<ApiResponse<PageTemplateDto>>> Create([FromBody] CreatePageTemplateCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command, ct); return CreatedResponse(result); }

    [HttpPut("{id:guid}")]
    [RequirePermission("Platform.Documents.Edit")]
    public async Task<ActionResult<ApiResponse<PageTemplateDto>>> Update(Guid id, [FromBody] UpdatePageTemplateCommand command, CancellationToken ct)
    { if (id != command.Id) return BadRequest(); var result = await Mediator.Send(command, ct); return OkResponse(result); }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Platform.Documents.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    { await Mediator.Send(new DeletePageTemplateCommand(id), ct); return OkResponse("Page template deleted"); }
}

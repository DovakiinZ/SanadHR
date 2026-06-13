using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.Commands.Documents;
using HR.Modules.Platform.DTOs.Documents;
using HR.Modules.Platform.Queries.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/documents")]
public class DocumentsController : BaseApiController
{
    /// <summary>Token explorer: the placeholders an admin can insert into a template.</summary>
    [HttpGet("token-catalog")]
    [RequirePermission("Platform.Documents.View")]
    public ActionResult<ApiResponse<List<TokenGroupDto>>> TokenCatalog() => OkResponse(DocumentTokens.Catalog);

    /// <summary>Live preview: resolve a template body's tokens against sample data → HTML.</summary>
    [HttpPost("preview-html")]
    [RequirePermission("Platform.Documents.View")]
    public ActionResult<ApiResponse<string>> PreviewHtml([FromBody] PreviewHtmlRequest req)
        => OkResponse<string>(Services.Documents.DocumentRenderer.ResolveTokens(req.Body ?? "", DocumentTokens.Sample));

    [HttpGet("templates")]
    [RequirePermission("Platform.Documents.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<DocumentTemplateDto>>>> GetTemplates([FromQuery] GetDocumentTemplatesQuery query, CancellationToken ct)
    { var result = await Mediator.Send(query, ct); return OkResponse(result); }

    [HttpGet("templates/{id:guid}")]
    [RequirePermission("Platform.Documents.View")]
    public async Task<ActionResult<ApiResponse<DocumentTemplateDto>>> GetTemplateById(Guid id, CancellationToken ct)
    { var result = await Mediator.Send(new GetDocumentTemplateByIdQuery(id), ct); return OkResponse(result); }

    [HttpPost("templates")]
    [RequirePermission("Platform.Documents.Create")]
    public async Task<ActionResult<ApiResponse<DocumentTemplateDto>>> CreateTemplate([FromBody] CreateDocumentTemplateCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command, ct); return CreatedResponse(result); }

    [HttpPut("templates/{id:guid}")]
    [RequirePermission("Platform.Documents.Edit")]
    public async Task<ActionResult<ApiResponse<DocumentTemplateDto>>> UpdateTemplate(Guid id, [FromBody] UpdateDocumentTemplateCommand command, CancellationToken ct)
    { if (id != command.Id) return BadRequest(); var result = await Mediator.Send(command, ct); return OkResponse(result); }

    [HttpDelete("templates/{id:guid}")]
    [RequirePermission("Platform.Documents.Delete")]
    public async Task<ActionResult<ApiResponse>> DeleteTemplate(Guid id, CancellationToken ct)
    { await Mediator.Send(new DeleteDocumentTemplateCommand(id), ct); return OkResponse("Template deleted"); }

    [HttpPost("templates/{id:guid}/publish")]
    [RequirePermission("Platform.Documents.Edit")]
    public async Task<ActionResult<ApiResponse<DocumentTemplateDto>>> PublishTemplate(Guid id, CancellationToken ct)
    { var result = await Mediator.Send(new PublishDocumentTemplateCommand(id), ct); return OkResponse(result); }

    [HttpPost("templates/{id:guid}/tokens")]
    [RequirePermission("Platform.Documents.Edit")]
    public async Task<ActionResult<ApiResponse<DocumentTemplateTokenDto>>> AddToken(Guid id, [FromBody] AddDocumentTokenCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command with { DocumentTemplateId = id }, ct); return CreatedResponse(result); }

    [HttpDelete("tokens/{tokenId:guid}")]
    [RequirePermission("Platform.Documents.Edit")]
    public async Task<ActionResult<ApiResponse>> DeleteToken(Guid tokenId, CancellationToken ct)
    { await Mediator.Send(new DeleteDocumentTokenCommand(tokenId), ct); return OkResponse("Token removed"); }

    [HttpGet("templates/{id:guid}/versions")]
    [RequirePermission("Platform.Documents.View")]
    public async Task<ActionResult<ApiResponse<List<DocumentTemplateVersionDto>>>> GetVersions(Guid id, CancellationToken ct)
    { var result = await Mediator.Send(new GetDocumentTemplateVersionsQuery(id), ct); return OkResponse(result); }

    [HttpPost("generate")]
    [RequirePermission("Platform.Documents.Create")]
    public async Task<ActionResult<ApiResponse<GeneratedDocumentDto>>> Generate([FromBody] GenerateDocumentCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command, ct); return CreatedResponse(result); }

    [HttpGet("generated")]
    [RequirePermission("Platform.Documents.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<GeneratedDocumentDto>>>> GetGenerated([FromQuery] GetGeneratedDocumentsQuery query, CancellationToken ct)
    { var result = await Mediator.Send(query, ct); return OkResponse(result); }

    // Branding
    [HttpGet("branding")]
    [RequirePermission("Platform.Documents.View")]
    public async Task<ActionResult<ApiResponse<List<CompanyBrandingDto>>>> GetBranding(CancellationToken ct)
    { var result = await Mediator.Send(new GetCompanyBrandingQuery(), ct); return OkResponse(result); }

    [HttpPost("branding")]
    [RequirePermission("Platform.Documents.Edit")]
    public async Task<ActionResult<ApiResponse<CompanyBrandingDto>>> SaveBranding([FromBody] SaveCompanyBrandingCommand command, CancellationToken ct)
    { var result = await Mediator.Send(command, ct); return OkResponse(result); }
}

using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.DTOs.Audit;
using HR.Modules.Platform.Queries.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/audit")]
public class AuditController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Platform.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<AuditEntryDto>>>> GetAll(
        [FromQuery] GetAuditEntriesQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }
}

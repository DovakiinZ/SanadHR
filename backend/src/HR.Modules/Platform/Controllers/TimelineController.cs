using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.DTOs.Timeline;
using HR.Modules.Platform.Queries.Timeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/timeline")]
public class TimelineController : BaseApiController
{
    [HttpGet("{entityType}/{entityId:guid}")]
    [RequirePermission("Platform.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<TimelineEventDto>>>> GetTimeline(
        string entityType, Guid entityId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetTimelineQuery
        {
            EntityType = entityType,
            EntityId = entityId,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, ct);
        return OkResponse(result);
    }
}

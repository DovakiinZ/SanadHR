using HR.Api.Controllers;
using HR.Application.Common.Models;
using HR.Modules.Platform.DTOs.MasterData;
using HR.Modules.Platform.Queries.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

/// <summary>
/// System-wide lookup API. Every module/form consumes tenant master data through
/// this single endpoint instead of building its own dropdown logic.
/// Read-only and available to any authenticated user (lookups are needed everywhere);
/// managing the data is gated separately by <c>MasterDataController</c>.
/// </summary>
[Authorize]
[Route("api/lookups")]
public class LookupsController : BaseApiController
{
    /// <summary>
    /// GET /api/lookups/{objectType} — e.g. job-titles, leave-types, allowance-types.
    /// Accepts either the kebab slug or the canonical name (JobTitle).
    /// </summary>
    [HttpGet("{objectType}")]
    public async Task<ActionResult<ApiResponse<List<LookupItemDto>>>> GetLookup(
        string objectType, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetLookupItemsQuery(objectType, !includeInactive), ct);
        return OkResponse(result);
    }
}

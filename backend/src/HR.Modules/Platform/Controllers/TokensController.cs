using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Platform.DTOs.Tokens;
using HR.Modules.Platform.Queries.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/tokens")]
public class TokensController : BaseApiController
{
    [HttpGet("categories")]
    [RequirePermission("Platform.View")]
    public async Task<ActionResult<ApiResponse<List<TokenCategoryDto>>>> GetCategories(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTokenCategoriesQuery(), ct);
        return OkResponse(result);
    }

    [HttpGet]
    [RequirePermission("Platform.View")]
    public async Task<ActionResult<ApiResponse<List<TokenDefinitionDto>>>> GetTokens(
        [FromQuery] string? category, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAvailableTokensQuery(category), ct);
        return OkResponse(result);
    }
}

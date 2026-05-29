using HR.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HR.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.Ok(data, message));

    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(T data, string? message = null)
        => StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Ok(data, message));

    protected ActionResult<ApiResponse> OkResponse(string? message = null)
        => Ok(ApiResponse.Ok(message));
}

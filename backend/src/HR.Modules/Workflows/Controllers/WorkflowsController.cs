using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Workflows.Controllers;

[Authorize]
[ApiController]
[Route("api/workflows")]
public class WorkflowsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => StatusCode(StatusCodes.Status501NotImplemented, new { message = "Workflows module not yet implemented" });
}

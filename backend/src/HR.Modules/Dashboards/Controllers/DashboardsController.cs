using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Dashboards.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboards")]
public class DashboardsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => StatusCode(StatusCodes.Status501NotImplemented, new { message = "Dashboards module not yet implemented" });
}

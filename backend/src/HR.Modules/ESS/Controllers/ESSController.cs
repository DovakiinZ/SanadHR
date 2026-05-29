using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.ESS.Controllers;

[Authorize]
[ApiController]
[Route("api/ess")]
public class ESSController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => StatusCode(StatusCodes.Status501NotImplemented, new { message = "ESS module not yet implemented" });
}

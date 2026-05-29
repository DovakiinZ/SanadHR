using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Payroll.Controllers;

[Authorize]
[ApiController]
[Route("api/payroll")]
public class PayrollController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => StatusCode(StatusCodes.Status501NotImplemented, new { message = "Payroll module not yet implemented" });
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Attendance.Controllers;

[Authorize]
[ApiController]
[Route("api/attendance")]
public class AttendanceController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => StatusCode(StatusCodes.Status501NotImplemented, new { message = "Attendance module not yet implemented" });
}

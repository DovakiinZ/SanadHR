using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Expenses.Controllers;

[Authorize]
[ApiController]
[Route("api/expenses")]
public class ExpensesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => StatusCode(StatusCodes.Status501NotImplemented, new { message = "Expenses module not yet implemented" });
}

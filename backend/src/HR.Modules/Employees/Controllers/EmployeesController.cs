using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Employees.Commands;
using HR.Modules.Employees.DTOs;
using HR.Modules.Employees.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Employees.Controllers;

[Authorize]
[Route("api/employees")]
public class EmployeesController : BaseApiController
{
    [HttpGet]
    [RequirePermission("Employees.View")]
    public async Task<ActionResult<ApiResponse<PaginatedList<EmployeeDto>>>> GetAll(
        [FromQuery] GetEmployeesQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Employees.View")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetEmployeeByIdQuery(id), ct);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("Employees.Create")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create(
        [FromBody] CreateEmployeeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Employees.Edit")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Update(
        Guid id, [FromBody] UpdateEmployeeCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        var result = await Mediator.Send(command, ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Employees.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteEmployeeCommand(id), ct);
        return OkResponse("Employee deleted");
    }
}

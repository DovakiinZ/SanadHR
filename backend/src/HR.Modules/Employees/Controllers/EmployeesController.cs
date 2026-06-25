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

    /// <summary>Export employees to .xlsx — field groups + filters; salary/bank gated by permission.</summary>
    [HttpPost("export")]
    [RequirePermission("Employees.View")]
    public async Task<IActionResult> Export([FromBody] ExportEmployeesQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return File(result.Content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.FileName);
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

    /// <summary>Preview an end-of-service settlement (Saudi Labor Law Art. 77/80/81 over the Art. 84/85
    /// gratuity) without persisting — drives the termination form's live award breakdown.</summary>
    [HttpPost("{id:guid}/settlement/preview")]
    [RequirePermission("Employees.Terminate")]
    public async Task<ActionResult<ApiResponse<SettlementResultDto>>> PreviewSettlement(
        Guid id, [FromBody] PreviewSettlementQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query with { EmployeeId = id }, ct);
        return OkResponse(result);
    }

    /// <summary>Compute + persist the settlement and transition the employee to Terminated/Resigned.</summary>
    [HttpPost("{id:guid}/terminate")]
    [RequirePermission("Employees.Terminate")]
    public async Task<ActionResult<ApiResponse<SettlementResultDto>>> Terminate(
        Guid id, [FromBody] TerminateEmployeeCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { EmployeeId = id }, ct);
        return OkResponse(result);
    }
}

using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Application.Engines.Settlement;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Controllers;

/// <summary>Employee restore (reinstatement) approval flow — request a former employee be reactivated,
/// then route through Manager → HR approval.</summary>
[Authorize]
[Route("api/restores")]
public class RestoresController : BaseApiController
{
    private readonly IRestoreWorkflow _workflow;
    private readonly ApplicationDbContext _db;

    public RestoresController(IRestoreWorkflow workflow, ApplicationDbContext db)
    {
        _workflow = workflow;
        _db = db;
    }

    [HttpPost("request")]
    [RequirePermission("Employees.Terminate")]
    public async Task<ActionResult<ApiResponse<RestoreDto>>> Request([FromBody] RestoreRequestDto req, CancellationToken ct)
    {
        var r = await _workflow.RequestAsync(req.EmployeeId, req.Reason, ct);
        return CreatedResponse(await MapAsync(r.Id, ct));
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<List<RestoreDto>>>> Pending(CancellationToken ct)
    {
        var list = await _workflow.GetPendingForCurrentUserAsync(ct);
        var dtos = new List<RestoreDto>();
        foreach (var r in list) dtos.Add(await MapAsync(r.Id, ct));
        return OkResponse(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RestoreDto>>> Get(Guid id, CancellationToken ct)
        => OkResponse(await MapAsync(id, ct));

    [HttpPost("{id:guid}/decide")]
    public async Task<ActionResult<ApiResponse<RestoreDto>>> Decide(Guid id, [FromBody] DecideDto body, CancellationToken ct)
    {
        await _workflow.DecideAsync(id, body.Approve, body.Comment, ct);
        return OkResponse(await MapAsync(id, ct));
    }

    private async Task<RestoreDto> MapAsync(Guid id, CancellationToken ct)
    {
        var r = await _workflow.GetAsync(id, ct);
        var emp = await _db.Employees.AsNoTracking().Where(e => e.Id == r.EmployeeId)
            .Select(e => new { Name = (e.FirstNameAr ?? e.FirstName) + " " + (e.LastNameAr ?? e.LastName), e.EmployeeNumber })
            .FirstOrDefaultAsync(ct);

        return new RestoreDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeName = emp?.Name?.Trim() ?? "",
            EmployeeNumber = emp?.EmployeeNumber ?? "",
            Status = r.Status.ToString(),
            CurrentStep = r.CurrentStep,
            Reason = r.Reason,
            RequestedAt = r.RequestedAt,
            ApprovedAt = r.ApprovedAt,
            RejectionReason = r.RejectionReason,
            Steps = r.ApprovalSteps.OrderBy(x => x.StepOrder).Select(x => new RestoreStepDto
            {
                Order = x.StepOrder, Role = x.Role.ToString(), Status = x.Status.ToString(),
                DecidedAt = x.DecidedAt, Comment = x.Comment,
            }).ToList(),
        };
    }
}

public class RestoreRequestDto
{
    public Guid EmployeeId { get; set; }
    public string? Reason { get; set; }
}

public class RestoreStepDto
{
    public int Order { get; set; }
    public string Role { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime? DecidedAt { get; set; }
    public string? Comment { get; set; }
}

public class RestoreDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";
    public string EmployeeNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public int CurrentStep { get; set; }
    public string? Reason { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public List<RestoreStepDto> Steps { get; set; } = new();
}

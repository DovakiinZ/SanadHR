using HR.Api.Controllers;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Modules.Platform.Controllers;

/// <summary>
/// Employee-facing Request Center. Only fully-provisioned request types are exposed,
/// so "if visible, it is usable". Authorization is enforced by the engine (employee
/// ownership / approver assignment), keeping endpoints simple and safe.
/// </summary>
[Authorize]
[Route("api/requests")]
public class RequestsController : BaseApiController
{
    private readonly IRequestEngine _engine;
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly ILeaveService _leave;

    public RequestsController(IRequestEngine engine, ApplicationDbContext db, ICurrentUserService user, ILeaveService leave)
    {
        _engine = engine; _db = db; _user = user; _leave = leave;
    }

    // ── Leave (generic sub-typed request) ───────────────────────────────────────

    /// <summary>Leave types from settings, with their rules and the employee's live balance.</summary>
    [HttpGet("leave-types")]
    public async Task<ActionResult<ApiResponse<List<LeaveTypeInfo>>>> GetLeaveTypes(CancellationToken ct)
    {
        var empId = await _db.Employees.Where(e => e.UserId == _user.UserId).Select(e => (Guid?)e.Id).FirstOrDefaultAsync(ct);
        if (empId is null) return OkResponse(new List<LeaveTypeInfo>());
        return OkResponse(await _leave.GetLeaveTypesAsync(empId.Value, ct));
    }

    /// <summary>Admin: an employee's leave types with their balances (entitled/used/remaining).</summary>
    [HttpGet("admin/leave-balances")]
    [HR.Api.Filters.RequirePermission("Platform.MasterData.Edit")]
    public async Task<ActionResult<ApiResponse<List<LeaveTypeInfo>>>> AdminLeaveBalances([FromQuery] Guid employeeId, CancellationToken ct)
        => OkResponse(await _leave.GetLeaveTypesAsync(employeeId, ct));

    /// <summary>Admin: set an employee's entitled / carried-forward balance for a leave type.</summary>
    [HttpPut("admin/leave-balances")]
    [HR.Api.Filters.RequirePermission("Platform.MasterData.Edit")]
    public async Task<ActionResult<ApiResponse>> SetLeaveBalance([FromBody] SetLeaveBalanceBody body, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var bal = await _db.LeaveBalances.FirstOrDefaultAsync(
            b => b.EmployeeId == body.EmployeeId && b.LeaveTypeId == body.LeaveTypeId && b.Year == year, ct);
        if (bal is null)
        {
            bal = new HR.Domain.Engines.Leave.LeaveBalance { EmployeeId = body.EmployeeId, LeaveTypeId = body.LeaveTypeId, Year = year };
            _db.LeaveBalances.Add(bal);
        }
        bal.EntitledDays = body.EntitledDays;
        bal.CarriedForwardDays = body.CarriedForwardDays;
        await _db.SaveChangesAsync(ct);
        return OkResponse("Balance updated");
    }

    /// <summary>Live preview for the leave wizard: days, balance before/after, next approver, validation.</summary>
    [HttpPost("leave/preview")]
    public async Task<ActionResult<ApiResponse<LeavePreview>>> LeavePreview([FromBody] LeavePreviewBody body, CancellationToken ct)
    {
        var empId = await _db.Employees.Where(e => e.UserId == _user.UserId).Select(e => (Guid?)e.Id).FirstOrDefaultAsync(ct);
        if (empId is null) return OkResponse(new LeavePreview { Errors = { "لا يوجد ملف موظف مرتبط بحسابك" } });
        DateTime? Parse(string? s) => DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var d) ? DateTime.SpecifyKind(d, DateTimeKind.Utc) : null;
        return OkResponse(await _leave.PreviewAsync(empId.Value, body.LeaveTypeId, Parse(body.StartDate), Parse(body.EndDate), body.HasAttachment, ct));
    }

    /// <summary>Idempotently provision the built-in system requests for this tenant.</summary>
    [HttpPost("seed-system")]
    [HR.Api.Filters.RequirePermission("Platform.MasterData.Create")]
    public async Task<ActionResult<ApiResponse<int>>> SeedSystem(CancellationToken ct)
    {
        var seeder = HttpContext.RequestServices.GetRequiredService<IRequestSeeder>();
        var n = await seeder.SeedSystemRequestsAsync(ct);
        return OkResponse(n, "System requests provisioned");
    }

    // ── Catalog (usable request types) ──────────────────────────────────────────

    [HttpGet("types")]
    public async Task<ActionResult<ApiResponse<List<RequestTypeDto>>>> GetTypes(CancellationToken ct)
    {
        var types = await _db.RequestTypes
            .Where(t => t.IsActive && t.FormDefinitionId != Guid.Empty)
            .OrderBy(t => t.Kind).ThenBy(t => t.SortOrder).ThenBy(t => t.NameAr)
            .Select(t => new RequestTypeDto
            {
                Id = t.Id, Code = t.Code, NameAr = t.NameAr, NameEn = t.NameEn,
                DescriptionAr = t.DescriptionAr, DescriptionEn = t.DescriptionEn,
                Kind = t.Kind.ToString(), CategoryId = t.CategoryId,
                Icon = t.Icon, Color = t.Color, IsSystem = t.IsSystem,
                HasWorkflow = t.WorkflowDefinitionId != null, GeneratesDocument = t.PrintTemplateId != null,
            }).ToListAsync(ct);
        return OkResponse(types);
    }

    [HttpGet("types/{id:guid}")]
    public async Task<ActionResult<ApiResponse<RequestTypeDetailDto>>> GetType(Guid id, CancellationToken ct)
    {
        var t = await _db.RequestTypes.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
        if (t is null) return NotFound(ApiResponse.Fail("Request type not found"));

        var fields = await _db.FormFields
            .Where(f => f.FormDefinitionId == t.FormDefinitionId)
            .OrderBy(f => f.SortOrder)
            .Select(f => new RequestFieldDto
            {
                Id = f.Id, Code = f.Code, NameAr = f.NameAr, NameEn = f.NameEn,
                FieldType = f.FieldType.ToString(), IsRequired = f.IsRequired,
                Placeholder = f.Placeholder, Options = f.Options, SortOrder = f.SortOrder,
            }).ToListAsync(ct);

        var isLeave = await _db.RequestImpactMappings.AnyAsync(m => m.RequestTypeId == id && m.AffectsLeaveBalance, ct);
        return OkResponse(new RequestTypeDetailDto
        {
            Id = t.Id, Code = t.Code, NameAr = t.NameAr, NameEn = t.NameEn,
            DescriptionAr = t.DescriptionAr, DescriptionEn = t.DescriptionEn,
            Kind = t.Kind.ToString(), FormDefinitionId = t.FormDefinitionId,
            Icon = t.Icon, Color = t.Color, Fields = fields, IsLeaveRequest = isLeave,
        });
    }

    // ── Submit / decide / cancel ────────────────────────────────────────────────

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RequestInstanceDto>>> Submit([FromBody] SubmitRequestBody body, CancellationToken ct)
    {
        var instance = await _engine.SubmitAsync(body.RequestTypeId, body.Values ?? new(), ct);
        return CreatedResponse(await MapInstanceAsync(instance.Id, ct));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<RequestInstanceDto>>> Approve(Guid id, [FromBody] DecisionBody? body, CancellationToken ct)
    {
        await _engine.DecideAsync(id, true, body?.Comment, ct);
        return OkResponse(await MapInstanceAsync(id, ct));
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ApiResponse<RequestInstanceDto>>> Reject(Guid id, [FromBody] DecisionBody? body, CancellationToken ct)
    {
        await _engine.DecideAsync(id, false, body?.Comment, ct);
        return OkResponse(await MapInstanceAsync(id, ct));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<RequestInstanceDto>>> Cancel(Guid id, CancellationToken ct)
    {
        await _engine.CancelAsync(id, ct);
        return OkResponse(await MapInstanceAsync(id, ct));
    }

    // ── My requests / approvals inbox / detail ──────────────────────────────────

    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<List<RequestInstanceDto>>>> Mine(CancellationToken ct)
    {
        var empId = await _db.Employees.Where(e => e.UserId == _user.UserId).Select(e => (Guid?)e.Id).FirstOrDefaultAsync(ct);
        if (empId is null) return OkResponse(new List<RequestInstanceDto>());
        var list = await _db.RequestInstances.Where(r => r.EmployeeId == empId)
            .OrderByDescending(r => r.SubmittedAt).Select(ProjectInstance).ToListAsync(ct);
        return OkResponse(list);
    }

    [HttpGet("inbox")]
    public async Task<ActionResult<ApiResponse<List<RequestInstanceDto>>>> Inbox(CancellationToken ct)
    {
        var pendingInstanceIds = _db.RequestApprovals
            .Where(a => a.AssignedToUserId == _user.UserId && a.Status == RequestApprovalStatus.Pending)
            .Select(a => a.RequestInstanceId);
        var list = await _db.RequestInstances
            .Where(r => pendingInstanceIds.Contains(r.Id) && (r.Status == RequestStatus.Submitted || r.Status == RequestStatus.InProgress))
            .OrderByDescending(r => r.SubmittedAt).Select(ProjectInstance).ToListAsync(ct);
        return OkResponse(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RequestInstanceDto>>> Detail(Guid id, CancellationToken ct)
    {
        var dto = await MapInstanceAsync(id, ct);
        return dto is null ? NotFound(ApiResponse.Fail("Request not found")) : OkResponse(dto);
    }

    /// <summary>Download the official PDF for a request (logo, CR/VAT, details, approvals, QR).</summary>
    [HttpGet("{id:guid}/document")]
    public async Task<IActionResult> Document(Guid id, CancellationToken ct)
    {
        var renderer = HttpContext.RequestServices.GetRequiredService<Services.Documents.IDocumentRenderer>();
        var (pdf, fileName) = await renderer.RenderRequestPdfAsync(id, ct);
        return File(pdf, "application/pdf", fileName);
    }

    // ── Projection helpers ──────────────────────────────────────────────────────

    private static readonly System.Linq.Expressions.Expression<Func<Domain.Engines.Requests.RequestInstance, RequestInstanceDto>> ProjectInstance =
        r => new RequestInstanceDto
        {
            Id = r.Id, RequestNumber = r.RequestNumber, RequestTypeId = r.RequestTypeId,
            RequestTypeNameAr = r.RequestType.NameAr, RequestTypeNameEn = r.RequestType.NameEn,
            Status = r.Status.ToString(), SubmittedAt = r.SubmittedAt, DecidedAt = r.DecidedAt,
            CurrentStepOrder = r.CurrentStepOrder,
            StartDate = r.StartDate, EndDate = r.EndDate, DaysCount = r.DaysCount,
            GeneratedDocumentId = r.GeneratedDocumentId,
        };

    private async Task<RequestInstanceDto?> MapInstanceAsync(Guid id, CancellationToken ct)
    {
        var dto = await _db.RequestInstances.Where(r => r.Id == id).Select(ProjectInstance).FirstOrDefaultAsync(ct);
        if (dto is null) return null;
        dto.Approvals = await _db.RequestApprovals.Where(a => a.RequestInstanceId == id).OrderBy(a => a.StepOrder)
            .Select(a => new RequestApprovalDto
            {
                StepOrder = a.StepOrder, StepNameAr = a.StepNameAr, StepNameEn = a.StepNameEn,
                Status = a.Status.ToString(), Comment = a.Comment, DecidedAt = a.DecidedAt,
            }).ToListAsync(ct);
        dto.History = await _db.RequestStatusHistories.Where(h => h.RequestInstanceId == id).OrderBy(h => h.At)
            .Select(h => new RequestHistoryDto { ToStatus = h.ToStatus.ToString(), NoteAr = h.NoteAr, NoteEn = h.NoteEn, At = h.At }).ToListAsync(ct);
        return dto;
    }
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed class SubmitRequestBody
{
    public Guid RequestTypeId { get; set; }
    public List<RequestValueInput>? Values { get; set; }
}
public sealed class DecisionBody { public string? Comment { get; set; } }

public sealed class RequestTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string Kind { get; set; } = null!;
    public Guid? CategoryId { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsSystem { get; set; }
    public bool HasWorkflow { get; set; }
    public bool GeneratesDocument { get; set; }
}

public sealed class RequestTypeDetailDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string Kind { get; set; } = null!;
    public Guid FormDefinitionId { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsLeaveRequest { get; set; }
    public List<RequestFieldDto> Fields { get; set; } = new();
}

public sealed class LeavePreviewBody
{
    public Guid LeaveTypeId { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public bool HasAttachment { get; set; }
}

public sealed class SetLeaveBalanceBody
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public decimal EntitledDays { get; set; }
    public decimal CarriedForwardDays { get; set; }
}

public sealed class RequestFieldDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string FieldType { get; set; } = null!;
    public bool IsRequired { get; set; }
    public string? Placeholder { get; set; }
    public string? Options { get; set; }
    public int SortOrder { get; set; }
}

public sealed class RequestInstanceDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = null!;
    public Guid RequestTypeId { get; set; }
    public string RequestTypeNameAr { get; set; } = null!;
    public string RequestTypeNameEn { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public int CurrentStepOrder { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? DaysCount { get; set; }
    public Guid? GeneratedDocumentId { get; set; }
    public List<RequestApprovalDto> Approvals { get; set; } = new();
    public List<RequestHistoryDto> History { get; set; } = new();
}

public sealed class RequestApprovalDto
{
    public int StepOrder { get; set; }
    public string StepNameAr { get; set; } = null!;
    public string StepNameEn { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime? DecidedAt { get; set; }
}

public sealed class RequestHistoryDto
{
    public string ToStatus { get; set; } = null!;
    public string? NoteAr { get; set; }
    public string? NoteEn { get; set; }
    public DateTime At { get; set; }
}

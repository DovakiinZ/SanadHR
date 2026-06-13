using HR.Api.Controllers;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Controllers;

/// <summary>
/// Approval Center — shows approval tasks assigned to the current user (or all, for admins).
/// Tasks are the request's RequestApproval steps; decisions drive the request engine.
/// </summary>
[Authorize]
[Route("api/approvals")]
public class ApprovalsController : BaseApiController
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly IRequestEngine _engine;

    public ApprovalsController(ApplicationDbContext db, ICurrentUserService user, IRequestEngine engine)
    {
        _db = db; _user = user; _engine = engine;
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<ApprovalTaskDto>>>> My([FromQuery] bool all, CancellationToken ct)
    {
        var isAdmin = await IsAdminAsync(ct);
        var q = _db.RequestApprovals.AsQueryable();
        if (!(all && isAdmin)) q = q.Where(a => a.AssignedToUserId == _user.UserId);
        var approvals = await q.ToListAsync(ct);

        var instIds = approvals.Select(a => a.RequestInstanceId).Distinct().ToList();
        var instances = await _db.RequestInstances.Include(r => r.RequestType)
            .Where(r => instIds.Contains(r.Id)).ToListAsync(ct);
        var empIds = instances.Select(i => i.EmployeeId).Distinct().ToList();
        var emps = await _db.Employees.Where(e => empIds.Contains(e.Id))
            .Select(e => new { e.Id, Name = (e.FirstNameAr ?? e.FirstName) + " " + (e.LastNameAr ?? e.LastName), e.DepartmentId }).ToListAsync(ct);
        var depIds = emps.Where(e => e.DepartmentId != null).Select(e => e.DepartmentId!.Value).Distinct().ToList();
        var deps = await _db.Departments.Where(d => depIds.Contains(d.Id)).Select(d => new { d.Id, d.NameAr }).ToListAsync(ct);
        var ltIds = instances.Where(i => i.LeaveTypeId != null).Select(i => i.LeaveTypeId!.Value).Distinct().ToList();
        var lts = await _db.MasterDataItems.Where(m => ltIds.Contains(m.Id)).Select(m => new { m.Id, m.NameAr }).ToListAsync(ct);

        var now = DateTime.UtcNow;
        var list = approvals.Select(a =>
        {
            var inst = instances.FirstOrDefault(i => i.Id == a.RequestInstanceId);
            var emp = inst is null ? null : emps.FirstOrDefault(e => e.Id == inst.EmployeeId);
            var dep = emp?.DepartmentId is { } dId ? deps.FirstOrDefault(d => d.Id == dId)?.NameAr : null;
            var lt = inst?.LeaveTypeId is { } lId ? lts.FirstOrDefault(x => x.Id == lId)?.NameAr : null;
            return new ApprovalTaskDto
            {
                Id = a.Id, RequestInstanceId = a.RequestInstanceId,
                RequestNumber = inst?.RequestNumber ?? "", RequestTypeNameAr = inst?.RequestType.NameAr ?? "", RequestTypeNameEn = inst?.RequestType.NameEn ?? "",
                OwnerName = emp?.Name?.Trim() ?? "—", Department = dep,
                SubmittedAt = inst?.SubmittedAt ?? now, DueAt = a.DueAt,
                Overdue = a.Status == RequestApprovalStatus.Pending && a.DueAt is { } due && due < now,
                Status = a.Status.ToString(), CurrentStepNameAr = a.StepNameAr,
                LeaveTypeName = lt, StartDate = inst?.StartDate, EndDate = inst?.EndDate, DaysCount = inst?.DaysCount,
            };
        }).OrderByDescending(x => x.SubmittedAt).ToList();

        return OkResponse(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ApprovalDetailDto>>> Detail(Guid id, CancellationToken ct)
    {
        var approval = await _db.RequestApprovals.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (approval is null) return NotFound(ApiResponse.Fail("Approval not found"));
        var dto = await BuildDetailAsync(approval.RequestInstanceId, ct);
        if (dto is null) return NotFound(ApiResponse.Fail("Request not found"));
        var isAdmin = await IsAdminAsync(ct);
        dto.CanAct = approval.Status == RequestApprovalStatus.Pending && (approval.AssignedToUserId == _user.UserId || isAdmin);
        dto.ApprovalId = approval.Id;
        return OkResponse(dto);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse>> Approve(Guid id, [FromBody] DecisionBody? body, CancellationToken ct)
        => await ActAsync(id, a => _engine.DecideAsync(a, true, body?.Comment, ct), ct);

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ApiResponse>> Reject(Guid id, [FromBody] DecisionBody? body, CancellationToken ct)
        => await ActAsync(id, a => _engine.DecideAsync(a, false, body?.Comment, ct), ct);

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<ApiResponse>> Return(Guid id, [FromBody] DecisionBody? body, CancellationToken ct)
        => await ActAsync(id, a => _engine.ReturnAsync(a, body?.Comment, ct), ct);

    // ── helpers ──

    private async Task<ActionResult<ApiResponse>> ActAsync(Guid approvalId, Func<Guid, Task> action, CancellationToken ct)
    {
        var requestInstanceId = await _db.RequestApprovals.Where(a => a.Id == approvalId).Select(a => (Guid?)a.RequestInstanceId).FirstOrDefaultAsync(ct);
        if (requestInstanceId is null) return NotFound(ApiResponse.Fail("Approval not found"));
        await action(requestInstanceId.Value);
        return OkResponse("Done");
    }

    private async Task<ApprovalDetailDto?> BuildDetailAsync(Guid requestInstanceId, CancellationToken ct)
    {
        var inst = await _db.RequestInstances.Include(r => r.RequestType).ThenInclude(t => t.ImpactMapping)
            .FirstOrDefaultAsync(r => r.Id == requestInstanceId, ct);
        if (inst is null) return null;

        var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == inst.EmployeeId, ct);
        var dep = emp?.DepartmentId is { } dId ? await _db.Departments.Where(d => d.Id == dId).Select(d => d.NameAr).FirstOrDefaultAsync(ct) : null;
        var job = emp?.JobTitleId is { } jId ? await _db.MasterDataItems.Where(m => m.Id == jId).Select(m => m.NameAr).FirstOrDefaultAsync(ct) : null;
        var lt = inst.LeaveTypeId is { } lId ? await _db.MasterDataItems.Where(m => m.Id == lId).Select(m => m.NameAr).FirstOrDefaultAsync(ct) : null;

        var approvals = await _db.RequestApprovals.Where(a => a.RequestInstanceId == inst.Id).OrderBy(a => a.StepOrder)
            .Select(a => new ApprovalStepDto { StepOrder = a.StepOrder, StepNameAr = a.StepNameAr, Status = a.Status.ToString(), Comment = a.Comment, DecidedAt = a.DecidedAt }).ToListAsync(ct);
        var history = await _db.RequestStatusHistories.Where(h => h.RequestInstanceId == inst.Id).OrderBy(h => h.At)
            .Select(h => new ApprovalHistoryDto { ToStatus = h.ToStatus.ToString(), NoteAr = h.NoteAr, At = h.At }).ToListAsync(ct);

        // Form values → details + attachments (with labels)
        var values = await _db.FormSubmissionValues.Where(v => v.FormSubmissionId == inst.FormSubmissionId).ToListAsync(ct);
        var fieldLabels = await _db.FormFields.Where(f => f.FormDefinitionId == inst.RequestType.FormDefinitionId)
            .Select(f => new { f.Code, f.NameAr }).ToListAsync(ct);
        string Label(string code) => fieldLabels.FirstOrDefault(f => f.Code == code)?.NameAr ?? code;
        // These are surfaced as structured fields already (leave type name + dates), so omit from the raw list.
        var structured = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "leaveType", "startDate", "endDate" };
        var details = values.Where(v => string.IsNullOrEmpty(v.FileUrl) && !string.IsNullOrWhiteSpace(v.Value) && !structured.Contains(v.FieldCode))
            .Select(v => new KvDto { Label = Label(v.FieldCode), Value = v.Value! }).ToList();
        var attachments = values.Where(v => !string.IsNullOrEmpty(v.FileUrl))
            .Select(v => new KvDto { Label = Label(v.FieldCode), Value = v.FileUrl! }).ToList();

        return new ApprovalDetailDto
        {
            RequestInstanceId = inst.Id, RequestNumber = inst.RequestNumber,
            RequestTypeNameAr = inst.RequestType.NameAr, RequestTypeNameEn = inst.RequestType.NameEn,
            Status = inst.Status.ToString(),
            OwnerName = emp is null ? "—" : $"{emp.FirstNameAr ?? emp.FirstName} {emp.LastNameAr ?? emp.LastName}".Trim(),
            OwnerNumber = emp?.EmployeeNumber, Department = dep, JobTitle = job,
            SubmittedAt = inst.SubmittedAt, LeaveTypeName = lt,
            StartDate = inst.StartDate, EndDate = inst.EndDate, DaysCount = inst.DaysCount,
            Impact = BuildImpact(inst, lt),
            Details = details, Attachments = attachments, Approvals = approvals, Timeline = history,
        };
    }

    private static List<string> BuildImpact(Domain.Engines.Requests.RequestInstance inst, string? leaveType)
    {
        var impact = inst.RequestType.ImpactMapping;
        var list = new List<string>();
        if (impact is null) return list;
        if (impact.AffectsLeaveBalance && inst.DaysCount is { } d)
            list.Add($"سيتم خصم {d} يوم من رصيد {leaveType ?? "الإجازة"}");
        if (impact.AffectsAttendance) list.Add("سيتم تسجيل الأيام كإجازة في الحضور");
        if (impact.AffectsPayroll) list.Add("سيكون هناك أثر على الراتب");
        if (impact.CreatesLoanRecord) list.Add("سيتم إنشاء سجل قرض");
        if (impact.GeneratesDocument) list.Add("سيتم إصدار مستند رسمي");
        if (impact.AffectsTimeline) list.Add("سيتم إنشاء حدث في المسار الزمني");
        list.Add("سيتم إشعار الموظف");
        return list;
    }

    private async Task<bool> IsAdminAsync(CancellationToken ct)
    {
        var tid = _user.TenantId;
        return await (from ur in _db.UserRoles.Where(x => x.UserId == _user.UserId)
                      join r in _db.Roles on ur.RoleId equals r.Id
                      where r.TenantId == tid && (r.IsSystemRole || EF.Functions.ILike(r.Name, "%admin%"))
                      select r.Id).AnyAsync(ct);
    }
}

// ── DTOs ──

public sealed class ApprovalTaskDto
{
    public Guid Id { get; set; }
    public Guid RequestInstanceId { get; set; }
    public string RequestNumber { get; set; } = "";
    public string RequestTypeNameAr { get; set; } = "";
    public string RequestTypeNameEn { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public string? Department { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public bool Overdue { get; set; }
    public string Status { get; set; } = "";
    public string CurrentStepNameAr { get; set; } = "";
    public string? LeaveTypeName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? DaysCount { get; set; }
}

public sealed class ApprovalDetailDto
{
    public Guid ApprovalId { get; set; }
    public Guid RequestInstanceId { get; set; }
    public string RequestNumber { get; set; } = "";
    public string RequestTypeNameAr { get; set; } = "";
    public string RequestTypeNameEn { get; set; } = "";
    public string Status { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public string? OwnerNumber { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? LeaveTypeName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? DaysCount { get; set; }
    public bool CanAct { get; set; }
    public List<string> Impact { get; set; } = new();
    public List<KvDto> Details { get; set; } = new();
    public List<KvDto> Attachments { get; set; } = new();
    public List<ApprovalStepDto> Approvals { get; set; } = new();
    public List<ApprovalHistoryDto> Timeline { get; set; } = new();
}

public sealed class KvDto { public string Label { get; set; } = ""; public string Value { get; set; } = ""; }
public sealed class ApprovalStepDto { public int StepOrder { get; set; } public string StepNameAr { get; set; } = ""; public string Status { get; set; } = ""; public string? Comment { get; set; } public DateTime? DecidedAt { get; set; } }
public sealed class ApprovalHistoryDto { public string ToStatus { get; set; } = ""; public string? NoteAr { get; set; } public DateTime At { get; set; } }

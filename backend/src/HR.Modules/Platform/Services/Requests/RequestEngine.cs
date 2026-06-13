using System.Text.Json;
using FluentValidation.Results;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Audit;
using HR.Application.Engines.Timeline;
using HR.Domain.Engines.Attendance;
using HR.Domain.Engines.Forms;
using HR.Domain.Engines.Leave;
using HR.Domain.Engines.Notifications;
using HR.Domain.Engines.Requests;
using HR.Domain.Engines.Workflows;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Requests;

public sealed class RequestEngine : IRequestEngine
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly ITimelineEngine _timeline;
    private readonly IAuditEngine _audit;

    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public RequestEngine(ApplicationDbContext db, ICurrentUserService user, ITimelineEngine timeline, IAuditEngine audit)
    {
        _db = db; _user = user; _timeline = timeline; _audit = audit;
    }

    // ── Submit ────────────────────────────────────────────────────────────────

    public async Task<RequestInstance> SubmitAsync(Guid requestTypeId, IReadOnlyList<RequestValueInput> values, CancellationToken ct)
    {
        var type = await _db.RequestTypes.Include(t => t.ImpactMapping)
            .FirstOrDefaultAsync(t => t.Id == requestTypeId && t.IsActive, ct)
            ?? throw new NotFoundException("RequestType", requestTypeId);

        if (type.FormDefinitionId == Guid.Empty)
            throw Invalid("form", "This request type has no form configured.");

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.UserId == _user.UserId, ct)
            ?? throw Invalid("employee", "No employee profile is linked to your account.");

        // Resolve field ids from codes (the FK is enforced; clients send codes).
        var fields = await _db.FormFields.Where(f => f.FormDefinitionId == type.FormDefinitionId).ToListAsync(ct);
        var fieldByCode = fields.ToDictionary(f => f.Code, f => f.Id, StringComparer.OrdinalIgnoreCase);

        // Validate required fields are present
        var provided = new HashSet<string>(values.Where(v => !string.IsNullOrWhiteSpace(v.Value)).Select(v => v.FieldCode), StringComparer.OrdinalIgnoreCase);
        var missing = fields.Where(f => f.IsRequired && !provided.Contains(f.Code)).Select(f => f.NameEn).ToList();
        if (missing.Count > 0)
            throw Invalid("fields", $"Missing required field(s): {string.Join(", ", missing)}");

        // 1) Persist the form submission
        var submission = new FormSubmission
        {
            FormDefinitionId = type.FormDefinitionId,
            SubmittedById = _user.UserId,
            SubmittedAt = DateTime.UtcNow,
            Status = FormSubmissionStatus.Submitted,
        };
        foreach (var v in values)
        {
            var fieldId = v.FormFieldId ?? (fieldByCode.TryGetValue(v.FieldCode, out var fid) ? fid : (Guid?)null);
            if (fieldId is null) continue; // ignore values for unknown fields (never break the FK)
            submission.Values.Add(new FormSubmissionValue { FormFieldId = fieldId.Value, FieldCode = v.FieldCode, Value = v.Value, FileUrl = v.FileUrl });
        }
        _db.FormSubmissions.Add(submission);

        // 2) Create the request instance (+ leave snapshot, object-driven)
        var instance = new RequestInstance
        {
            RequestTypeId = type.Id,
            RequestNumber = await NextRequestNumberAsync(ct),
            EmployeeId = employee.Id,
            FormSubmissionId = submission.Id,
            Status = RequestStatus.Submitted,
            SubmittedAt = DateTime.UtcNow,
            LeaveTypeId = type.LeaveTypeId,
        };
        if (type.LeaveTypeId is not null)
        {
            instance.StartDate = ParseDate(Val(values, RequestFieldCodes.StartDate));
            instance.EndDate = ParseDate(Val(values, RequestFieldCodes.EndDate));
            if (instance.StartDate is { } sd && instance.EndDate is { } ed && ed >= sd)
                instance.DaysCount = (decimal)((ed.Date - sd.Date).Days + 1);
        }
        _db.RequestInstances.Add(instance);

        // 3) Resolve approval chain from the linked workflow + create a workflow instance
        var chain = await BuildApprovalChainAsync(type, employee, ct);
        if (chain.Count > 0)
        {
            var (wfInstanceId, _) = await StartWorkflowAsync(type, instance.Id, ct);
            instance.WorkflowInstanceId = wfInstanceId;
            for (int i = 0; i < chain.Count; i++)
            {
                chain[i].RequestInstanceId = instance.Id;
                chain[i].StepOrder = i + 1;
                chain[i].Status = i == 0 ? RequestApprovalStatus.Pending : RequestApprovalStatus.Pending;
                _db.RequestApprovals.Add(chain[i]);
            }
            instance.Status = RequestStatus.InProgress;
            instance.CurrentStepOrder = 1;
        }

        AddHistory(instance, null, instance.Status, "تم تقديم الطلب", "Request submitted");
        await _db.SaveChangesAsync(ct);

        // 4) Side records: timeline, audit, notifications
        await _timeline.PublishEvent("Requests", "RequestInstance", instance.Id, "Submitted",
            $"Request {instance.RequestNumber} submitted", $"تم تقديم الطلب {instance.RequestNumber}", new { type.Code }, ct);
        await _audit.LogChange("RequestInstance", instance.Id, "Submitted", null, new { instance.RequestNumber, type.Code }, ct);

        if (chain.Count > 0)
        {
            var first = chain[0];
            if (first.AssignedToUserId is { } approverId)
                await NotifyAsync(approverId, "طلب بانتظار موافقتك", "A request needs your approval",
                    $"{type.NameAr} — {instance.RequestNumber}", $"{type.NameEn} — {instance.RequestNumber}", "RequestApproval", instance.Id, ct);
        }
        else
        {
            await ApplyImpactsAsync(instance, type, employee, ct);
        }

        await _db.SaveChangesAsync(ct);
        return instance;
    }

    // ── Decide (approve / reject) ───────────────────────────────────────────────

    public async Task<RequestInstance> DecideAsync(Guid requestInstanceId, bool approved, string? comment, CancellationToken ct)
    {
        var instance = await _db.RequestInstances
            .Include(r => r.Approvals)
            .Include(r => r.RequestType).ThenInclude(t => t.ImpactMapping)
            .FirstOrDefaultAsync(r => r.Id == requestInstanceId, ct)
            ?? throw new NotFoundException("RequestInstance", requestInstanceId);

        if (instance.Status is not (RequestStatus.Submitted or RequestStatus.InProgress))
            throw Invalid("status", "This request is not awaiting a decision.");

        var step = instance.Approvals
            .Where(a => a.Status == RequestApprovalStatus.Pending)
            .OrderBy(a => a.StepOrder).FirstOrDefault()
            ?? throw Invalid("step", "No pending approval step.");

        var isAdmin = await IsAdminAsync(ct);
        if (step.AssignedToUserId != _user.UserId && !isAdmin)
            throw new ForbiddenException("This approval is not assigned to you.");

        step.DecidedByUserId = _user.UserId;
        step.DecidedAt = DateTime.UtcNow;
        step.Comment = comment;

        if (!approved)
        {
            step.Status = RequestApprovalStatus.Rejected;
            await TransitionAsync(instance, RequestStatus.Rejected, comment, "تم رفض الطلب", "Request rejected", ct);
            await CloseWorkflowAsync(instance, WorkflowStatus.Rejected, ct);
            await NotifySubmitterAsync(instance, "تم رفض طلبك", "Your request was rejected", ct);
            await _db.SaveChangesAsync(ct);
            return instance;
        }

        step.Status = RequestApprovalStatus.Approved;
        var next = instance.Approvals
            .Where(a => a.Status == RequestApprovalStatus.Pending)
            .OrderBy(a => a.StepOrder).FirstOrDefault();

        if (next is not null)
        {
            instance.CurrentStepOrder = next.StepOrder;
            await TransitionAsync(instance, RequestStatus.InProgress, comment, "تمت موافقة خطوة", "Step approved", ct);
            if (next.AssignedToUserId is { } nextApprover)
                await NotifyAsync(nextApprover, "طلب بانتظار موافقتك", "A request needs your approval",
                    $"{instance.RequestType.NameAr} — {instance.RequestNumber}", $"{instance.RequestType.NameEn} — {instance.RequestNumber}", "RequestApproval", instance.Id, ct);
            await _db.SaveChangesAsync(ct);
            return instance;
        }

        // Final approval → apply impacts
        await TransitionAsync(instance, RequestStatus.Approved, comment, "تمت الموافقة على الطلب", "Request approved", ct);
        await CloseWorkflowAsync(instance, WorkflowStatus.Completed, ct);
        var employee = await _db.Employees.FirstAsync(e => e.Id == instance.EmployeeId, ct);
        await ApplyImpactsAsync(instance, instance.RequestType, employee, ct);
        await NotifySubmitterAsync(instance, "تمت الموافقة على طلبك", "Your request was approved", ct);
        await _db.SaveChangesAsync(ct);
        return instance;
    }

    public async Task<RequestInstance> CancelAsync(Guid requestInstanceId, CancellationToken ct)
    {
        var instance = await _db.RequestInstances.FirstOrDefaultAsync(r => r.Id == requestInstanceId, ct)
            ?? throw new NotFoundException("RequestInstance", requestInstanceId);
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.UserId == _user.UserId, ct);
        if (employee is null || instance.EmployeeId != employee.Id)
            throw new ForbiddenException("You can only cancel your own requests.");
        if (instance.Status is RequestStatus.Approved or RequestStatus.Rejected or RequestStatus.Cancelled)
            throw Invalid("status", "This request can no longer be cancelled.");

        await TransitionAsync(instance, RequestStatus.Cancelled, null, "تم إلغاء الطلب", "Request cancelled", ct);
        await CloseWorkflowAsync(instance, WorkflowStatus.Cancelled, ct);
        await _db.SaveChangesAsync(ct);
        return instance;
    }

    // ── Impact engine (deterministic, driven by RequestImpactMapping) ───────────

    private async Task ApplyImpactsAsync(RequestInstance instance, RequestType type, HR.Modules.Employees.Entities.Employee employee, CancellationToken ct)
    {
        var impact = type.ImpactMapping;
        if (impact is null) return;

        if (impact.AffectsLeaveBalance && instance.LeaveTypeId is { } leaveTypeId && instance.DaysCount is { } days && days > 0)
        {
            var year = (instance.StartDate ?? DateTime.UtcNow).Year;
            var bal = await _db.LeaveBalances.FirstOrDefaultAsync(
                b => b.EmployeeId == employee.Id && b.LeaveTypeId == leaveTypeId && b.Year == year, ct);
            if (bal is null)
            {
                bal = new LeaveBalance { EmployeeId = employee.Id, LeaveTypeId = leaveTypeId, Year = year, EntitledDays = 30m, UsedDays = 0m };
                _db.LeaveBalances.Add(bal);
            }
            bal.UsedDays += days;
        }

        if (impact.AffectsAttendance && instance.StartDate is { } start && instance.EndDate is { } end && end >= start)
        {
            for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            {
                _db.AttendanceRecords.Add(new AttendanceRecord
                {
                    EmployeeId = employee.Id,
                    Date = DateTime.SpecifyKind(d, DateTimeKind.Utc),
                    Status = AttendanceStatus.OnLeave,
                    Source = "LeaveRequest",
                    ReferenceId = instance.Id,
                });
            }
        }

        if (impact.GeneratesDocument && type.PrintTemplateId is { } templateId)
        {
            var doc = new HR.Domain.Engines.Documents.GeneratedDocument
            {
                DocumentTemplateId = templateId,
                EntityType = "RequestInstance",
                EntityId = instance.Id,
                Status = HR.Domain.Enums.DocumentGenerationStatus.Completed,
                OutputFormat = HR.Domain.Enums.DocumentOutputFormat.Pdf,
                FileName = $"{type.Code}-{instance.RequestNumber}.pdf",
                TokenValues = JsonSerializer.Serialize(new { instance.RequestNumber, employee = $"{employee.FirstName} {employee.LastName}" }),
                GeneratedAt = DateTime.UtcNow,
                GeneratedById = _user.UserId,
            };
            _db.Set<HR.Domain.Engines.Documents.GeneratedDocument>().Add(doc);
            instance.GeneratedDocumentId = doc.Id;
        }

        if (impact.AffectsTimeline)
            await _timeline.PublishEvent("Requests", "Employee", employee.Id, "RequestApproved",
                $"{type.NameEn} approved", $"تمت الموافقة على {type.NameAr}", new { instance.RequestNumber }, ct);
        if (impact.AffectsAudit)
            await _audit.LogChange("RequestInstance", instance.Id, "Approved", null, new { instance.RequestNumber, type.Code }, ct);
    }

    // ── Approval-chain resolution from the linked workflow ──────────────────────

    private async Task<List<RequestApproval>> BuildApprovalChainAsync(RequestType type, HR.Modules.Employees.Entities.Employee employee, CancellationToken ct)
    {
        var result = new List<RequestApproval>();
        if (type.WorkflowDefinitionId is not { } wfId) return result;

        var version = await _db.WorkflowVersions
            .Where(v => v.WorkflowDefinitionId == wfId && v.IsPublished)
            .OrderByDescending(v => v.VersionNumber).FirstOrDefaultAsync(ct);
        if (version?.Configuration is null) return result;

        WorkflowChainConfig? cfg;
        try { cfg = JsonSerializer.Deserialize<WorkflowChainConfig>(version.Configuration, Json); }
        catch { cfg = null; }
        if (cfg?.Steps is null) return result;

        foreach (var s in cfg.Steps)
        {
            var approverType = (ApproverType)s.ApproverType;
            var assignee = await ResolveApproverAsync(approverType, s.SpecificUserId, employee, ct);
            result.Add(new RequestApproval
            {
                StepNameAr = s.NameAr,
                StepNameEn = s.NameEn,
                ApproverType = approverType,
                AssignedToUserId = assignee,
                Status = RequestApprovalStatus.Pending,
            });
        }
        return result;
    }

    private async Task<Guid?> ResolveApproverAsync(ApproverType type, Guid? specificUserId, HR.Modules.Employees.Entities.Employee employee, CancellationToken ct)
    {
        Guid? resolved = type switch
        {
            ApproverType.SpecificUser => specificUserId,
            ApproverType.DirectManager => await ManagerUserAsync(employee.ManagerId, ct),
            ApproverType.DepartmentHead => await DepartmentHeadUserAsync(employee.DepartmentId, ct),
            ApproverType.HrManager => await UserByRoleKeywordAsync("HR", ct),
            _ => null,
        };
        // Guarantee progress: fall back to a tenant admin so a chain never dead-ends.
        return resolved ?? await AdminUserIdAsync(ct);
    }

    private async Task<Guid?> ManagerUserAsync(Guid? managerEmployeeId, CancellationToken ct)
    {
        if (managerEmployeeId is not { } id) return null;
        return await _db.Employees.Where(e => e.Id == id).Select(e => e.UserId).FirstOrDefaultAsync(ct);
    }

    private async Task<Guid?> DepartmentHeadUserAsync(Guid? departmentId, CancellationToken ct)
    {
        if (departmentId is not { } id) return null;
        var managerEmpId = await _db.Departments.Where(d => d.Id == id).Select(d => d.ManagerId).FirstOrDefaultAsync(ct);
        return await ManagerUserAsync(managerEmpId, ct);
    }

    private async Task<Guid?> UserByRoleKeywordAsync(string keyword, CancellationToken ct)
    {
        var tid = _user.TenantId;
        return await (from u in _db.Users.Where(u => u.TenantId == tid && u.IsActive)
                      join ur in _db.UserRoles on u.Id equals ur.UserId
                      join r in _db.Roles on ur.RoleId equals r.Id
                      where EF.Functions.ILike(r.Name, $"%{keyword}%")
                      select (Guid?)u.Id).FirstOrDefaultAsync(ct);
    }

    private async Task<Guid?> AdminUserIdAsync(CancellationToken ct)
    {
        var tid = _user.TenantId;
        var admin = await (from u in _db.Users.Where(u => u.TenantId == tid && u.IsActive)
                           join ur in _db.UserRoles on u.Id equals ur.UserId
                           join r in _db.Roles on ur.RoleId equals r.Id
                           where r.IsSystemRole || EF.Functions.ILike(r.Name, "%admin%")
                           select (Guid?)u.Id).FirstOrDefaultAsync(ct);
        return admin ?? await _db.Users.Where(u => u.TenantId == tid && u.IsActive).Select(u => (Guid?)u.Id).FirstOrDefaultAsync(ct);
    }

    private async Task<bool> IsAdminAsync(CancellationToken ct)
    {
        var tid = _user.TenantId;
        return await (from ur in _db.UserRoles.Where(x => x.UserId == _user.UserId)
                      join r in _db.Roles on ur.RoleId equals r.Id
                      where r.TenantId == tid && (r.IsSystemRole || EF.Functions.ILike(r.Name, "%admin%"))
                      select r.Id).AnyAsync(ct);
    }

    // ── Workflow instance (traceability; chain is driven by RequestApproval) ────

    private async Task<(Guid instanceId, Guid versionId)> StartWorkflowAsync(RequestType type, Guid requestInstanceId, CancellationToken ct)
    {
        var versionId = await _db.WorkflowVersions
            .Where(v => v.WorkflowDefinitionId == type.WorkflowDefinitionId && v.IsPublished)
            .OrderByDescending(v => v.VersionNumber).Select(v => v.Id).FirstOrDefaultAsync(ct);

        var wf = new WorkflowInstance
        {
            WorkflowDefinitionId = type.WorkflowDefinitionId!.Value,
            WorkflowVersionId = versionId,
            EntityType = "RequestInstance",
            EntityId = requestInstanceId,
            Status = WorkflowStatus.Active,
            StartedAt = DateTime.UtcNow,
        };
        _db.WorkflowInstances.Add(wf);
        return (wf.Id, versionId);
    }

    private async Task CloseWorkflowAsync(RequestInstance instance, WorkflowStatus status, CancellationToken ct)
    {
        if (instance.WorkflowInstanceId is not { } id) return;
        var wf = await _db.WorkflowInstances.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (wf is null) return;
        wf.Status = status;
        wf.CompletedAt = DateTime.UtcNow;
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task TransitionAsync(RequestInstance instance, RequestStatus to, string? note, string noteAr, string noteEn, CancellationToken ct)
    {
        var from = instance.Status;
        instance.Status = to;
        if (to is RequestStatus.Approved or RequestStatus.Rejected or RequestStatus.Cancelled)
            instance.DecidedAt = DateTime.UtcNow;
        instance.DecisionNote = note ?? instance.DecisionNote;
        AddHistory(instance, from, to, noteAr, noteEn);
        await _timeline.PublishEvent("Requests", "RequestInstance", instance.Id, to.ToString(), noteEn, noteAr, null, ct);
        await _audit.LogChange("RequestInstance", instance.Id, to.ToString(), new { from }, new { to }, ct);
    }

    private void AddHistory(RequestInstance instance, RequestStatus? from, RequestStatus to, string noteAr, string noteEn)
        => _db.RequestStatusHistories.Add(new RequestStatusHistory
        {
            RequestInstanceId = instance.Id, FromStatus = from, ToStatus = to,
            ActorUserId = _user.UserId, NoteAr = noteAr, NoteEn = noteEn, At = DateTime.UtcNow,
        });

    private async Task NotifySubmitterAsync(RequestInstance instance, string titleAr, string titleEn, CancellationToken ct)
    {
        var submitterUserId = await _db.Employees.Where(e => e.Id == instance.EmployeeId).Select(e => e.UserId).FirstOrDefaultAsync(ct);
        if (submitterUserId is { } uid)
            await NotifyAsync(uid, titleAr, titleEn, instance.RequestNumber, instance.RequestNumber, "RequestDecision", instance.Id, ct);
    }

    private Task NotifyAsync(Guid userId, string titleAr, string titleEn, string bodyAr, string bodyEn, string category, Guid entityId, CancellationToken ct)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId, TitleAr = titleAr, TitleEn = titleEn, BodyAr = bodyAr, BodyEn = bodyEn,
            Category = category, EntityId = entityId, Link = "/requests", IsRead = false,
        });
        return Task.CompletedTask;
    }

    private async Task<string> NextRequestNumberAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _db.RequestInstances.CountAsync(ct);
        return $"REQ-{year}-{(count + 1):D6}";
    }

    private static string? Val(IReadOnlyList<RequestValueInput> values, string code)
        => values.FirstOrDefault(v => string.Equals(v.FieldCode, code, StringComparison.OrdinalIgnoreCase))?.Value;

    private static DateTime? ParseDate(string? raw)
        => DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var d)
            ? DateTime.SpecifyKind(d, DateTimeKind.Utc) : null;

    private static ValidationException Invalid(string field, string message)
        => new(new[] { new ValidationFailure(field, message) });
}

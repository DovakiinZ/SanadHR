using HR.Application.Common.Interfaces;
using HR.Application.Engines.Audit;
using HR.Application.Engines.Timeline;
using HR.Domain.Engines.Attendance;
using HR.Domain.Engines.Documents;
using HR.Domain.Engines.Leave;
using HR.Domain.Engines.MasterData;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Leaves;
using HR.Modules.Platform.Services.Documents;
using HR.Modules.Platform.Services.Notifications;
using HR.Modules.Platform.Services.Requests;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Leaves;

public interface ILeaveRecordService
{
    Task<List<LeaveRecordListDto>> ListAsync(LeaveFilter filter, CancellationToken ct);
    Task<LeaveDetailDto?> GetDetailAsync(Guid id, CancellationToken ct);
    Task<int> AssignAsync(AssignLeaveRequest req, CancellationToken ct);
    Task EditAsync(Guid id, EditLeaveRequest req, CancellationToken ct);
    Task CancelAsync(Guid id, string? reason, CancellationToken ct);
    Task<List<LeaveBalanceDto>> GetEmployeeBalanceAsync(Guid employeeId, CancellationToken ct);
    Task<(byte[] pdf, string fileName)> PrintAsync(Guid id, CancellationToken ct);
}

public sealed class LeaveRecordService : ILeaveRecordService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly ILeaveService _leave;
    private readonly ITimelineEngine _timeline;
    private readonly IAuditEngine _audit;
    private readonly INotificationService _notify;
    private readonly IDocumentRenderer _renderer;

    public LeaveRecordService(ApplicationDbContext db, ICurrentUserService user, ILeaveService leave,
        ITimelineEngine timeline, IAuditEngine audit, INotificationService notify, IDocumentRenderer renderer)
    {
        _db = db; _user = user; _leave = leave; _timeline = timeline; _audit = audit; _notify = notify; _renderer = renderer;
    }

    private static DateTime Utc(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);

    // ── List (self-heals from approved leave requests) ──────────────────────────

    public async Task<List<LeaveRecordListDto>> ListAsync(LeaveFilter f, CancellationToken ct)
    {
        await BackfillFromRequestsAsync(ct);

        var q = _db.LeaveRecords.AsNoTracking().AsQueryable();
        if (f.Mine && f.MyEmployeeId is { } me) q = q.Where(r => r.EmployeeId == me);
        if (f.EmployeeId is { } eid) q = q.Where(r => r.EmployeeId == eid);
        if (f.LeaveTypeId is { } lt) q = q.Where(r => r.LeaveTypeId == lt);
        if (f.From is { } from) q = q.Where(r => r.EndDate >= Utc(from));
        if (f.To is { } to) q = q.Where(r => r.StartDate <= Utc(to));
        if (!string.IsNullOrWhiteSpace(f.Status) && Enum.TryParse<LeaveRecordStatus>(f.Status, true, out var st)) q = q.Where(r => r.Status == st);
        if (!string.IsNullOrWhiteSpace(f.Source) && Enum.TryParse<LeaveRecordSource>(f.Source, true, out var src)) q = q.Where(r => r.Source == src);

        var rows = await (from r in q
                          join e in _db.Employees on r.EmployeeId equals e.Id into ej
                          from e in ej.DefaultIfEmpty()
                          join d in _db.Departments on e.DepartmentId equals d.Id into dj
                          from d in dj.DefaultIfEmpty()
                          join b in _db.Branch on e.BranchId equals b.Id into bj
                          from b in bj.DefaultIfEmpty()
                          join p in _db.Positions on e.JobTitleId equals p.Id into pj
                          from p in pj.DefaultIfEmpty()
                          join mdi in _db.MasterDataItems on r.LeaveTypeId equals mdi.Id into ltj
                          from ltype in ltj.DefaultIfEmpty()
                          join ri in _db.RequestInstances on r.RequestInstanceId equals ri.Id into rij
                          from ri in rij.DefaultIfEmpty()
                          where (f.DepartmentId == null || e.DepartmentId == f.DepartmentId)
                             && (f.BranchId == null || e.BranchId == f.BranchId)
                          orderby r.StartDate descending
                          select new { r, e, d, b, p, lt = ltype, ri }).ToListAsync(ct);

        // Resolve approver names in a second pass (avoid a heavy join on Users).
        var approverIds = rows.Where(x => x.r.ApprovedByUserId != null).Select(x => x.r.ApprovedByUserId!.Value).Distinct().ToList();
        var approverNames = await UserNamesAsync(approverIds, ct);

        return rows.Select(x => Map(x.r, x.e, x.d, x.b, x.p, x.lt, x.ri, approverNames)).ToList();
    }

    private LeaveRecordListDto Map(LeaveRecord r,
        HR.Modules.Employees.Entities.Employee? e, HR.Modules.Core.Entities.Department? d, HR.Modules.Core.Entities.Branch? b,
        HR.Domain.Engines.CompanyConfig.Position? p, MasterDataItem? lt, HR.Domain.Engines.Requests.RequestInstance? ri,
        IReadOnlyDictionary<Guid, string> approverNames) => new()
    {
        Id = r.Id, RecordNumber = r.RecordNumber, EmployeeId = r.EmployeeId,
        EmployeeName = e != null ? ((e.FirstNameAr ?? e.FirstName) + " " + (e.LastNameAr ?? e.LastName)) : null,
        EmployeeNumber = e?.EmployeeNumber,
        DepartmentName = d != null ? (d.NameAr ?? d.Name) : null,
        BranchName = b != null ? (b.NameAr ?? b.Name) : null,
        JobTitleName = p != null ? (p.NameAr ?? p.NameEn) : null,
        LeaveTypeId = r.LeaveTypeId, LeaveTypeName = lt != null ? (lt.NameAr ?? lt.NameEn) : null,
        StartDate = r.StartDate, EndDate = r.EndDate, DaysCount = r.DaysCount,
        AffectsBalance = r.AffectsBalance, BalanceBefore = r.BalanceBefore, BalanceAfter = r.BalanceAfter,
        Status = r.Status.ToString(), Source = r.Source.ToString(),
        RequestInstanceId = r.RequestInstanceId, RequestNumber = ri?.RequestNumber,
        ApprovedAt = r.ApprovedAt,
        ApprovedByName = r.ApprovedByUserId is { } uid && approverNames.TryGetValue(uid, out var n) ? n : null,
        Notes = r.Notes, HasAttachment = !string.IsNullOrWhiteSpace(r.AttachmentUrl), CanceledAt = r.CanceledAt,
    };

    // ── Detail ──────────────────────────────────────────────────────────────────

    public async Task<LeaveDetailDto?> GetDetailAsync(Guid id, CancellationToken ct)
    {
        var r = await _db.LeaveRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return null;

        var e = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == r.EmployeeId, ct);
        var d = e?.DepartmentId is { } dep ? await _db.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dep, ct) : null;
        var b = e?.BranchId is { } br ? await _db.Branch.AsNoTracking().FirstOrDefaultAsync(x => x.Id == br, ct) : null;
        var p = e?.JobTitleId is { } jt ? await _db.Positions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == jt, ct) : null;
        var lt = await _db.MasterDataItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == r.LeaveTypeId, ct);
        var ri = r.RequestInstanceId is { } rid ? await _db.RequestInstances.AsNoTracking().FirstOrDefaultAsync(x => x.Id == rid, ct) : null;
        var nationality = e?.NationalityId is { } nat ? await _db.MasterDataItems.AsNoTracking().Where(m => m.Id == nat).Select(m => m.NameAr).FirstOrDefaultAsync(ct) : null;
        var approverNames = r.ApprovedByUserId is { } uid ? await UserNamesAsync(new() { uid }, ct) : new Dictionary<Guid, string>();

        var attendanceDays = await _db.AttendanceRecords.AsNoTracking()
            .Where(a => a.EmployeeId == r.EmployeeId && a.Date >= r.StartDate && a.Date <= r.EndDate && a.Status == AttendanceStatus.OnLeave)
            .Select(a => a.Date).OrderBy(x => x).ToListAsync(ct);

        var audit = await _db.LeaveAuditLogs.AsNoTracking().Where(a => a.LeaveRecordId == id)
            .OrderByDescending(a => a.At)
            .Select(a => new LeaveTimelineDto { Action = a.Action, Description = a.DetailsAr ?? a.DetailsEn, At = a.At })
            .ToListAsync(ct);

        var tl = await _timeline.GetTimeline("LeaveRecord", id, 1, 50, ct);
        var timeline = tl.Items.Select(t => new LeaveTimelineDto { Action = t.Action, Description = t.DescriptionAr ?? t.DescriptionEn, At = t.OccurredAt }).ToList();

        var rules = _leave.GetRules(lt?.MetadataJson);

        return new LeaveDetailDto
        {
            Record = Map(r, e, d, b, p, lt, ri, approverNames),
            Nationality = nationality, NationalId = e?.NationalId,
            DaysDeducted = r.AffectsBalance ? r.DaysCount : 0m,
            AttendanceDays = attendanceDays,
            AffectsPayroll = rules.AffectsPayroll,
            AttachmentUrl = r.AttachmentUrl,
            Audit = audit, Timeline = timeline,
        };
    }

    // ── Assign (direct HR leave) ─────────────────────────────────────────────────

    public async Task<int> AssignAsync(AssignLeaveRequest req, CancellationToken ct)
    {
        var leaveItem = await _db.MasterDataItems.FirstOrDefaultAsync(m => m.Id == req.LeaveTypeId && m.ObjectType == MasterDataObjectType.LeaveType, ct)
            ?? throw new InvalidOperationException("نوع الإجازة غير موجود");
        var rules = _leave.GetRules(leaveItem.MetadataJson);
        var start = Utc(req.StartDate);
        var end = Utc(req.EndDate);
        if (end < start) throw new InvalidOperationException("تاريخ النهاية قبل تاريخ البداية");
        var days = _leave.ComputeDays(start, end, rules);

        var empIds = await ResolveTargetEmployeesAsync(req, ct);
        if (empIds.Count == 0) throw new InvalidOperationException("لا يوجد موظفون مطابقون للنطاق المحدد");

        var assignment = new LeaveAssignment
        {
            LeaveTypeId = req.LeaveTypeId, StartDate = start, EndDate = end, DaysCount = days,
            TargetScope = req.Scope, DepartmentId = req.DepartmentId, BranchId = req.BranchId, JobTitleId = req.JobTitleId,
            Notes = req.Notes, AttachmentUrl = req.AttachmentUrl, AssignedCount = empIds.Count, CreatedByUserId = _user.UserId,
        };
        _db.LeaveAssignments.Add(assignment);

        var year = start.Year;
        var baseNumber = await _db.LeaveRecords.CountAsync(ct);
        var i = 0;
        foreach (var empId in empIds)
        {
            var affects = rules.Paid && days > 0;
            var before = await RemainingWithDefaultAsync(empId, req.LeaveTypeId, year, (decimal)rules.AnnualBalance, ct);
            var after = before;
            if (affects) after = await AdjustBalanceAsync(empId, req.LeaveTypeId, year, days, (decimal)(rules.AnnualBalance), "تعيين إجازة", null, ct);

            var record = new LeaveRecord
            {
                RecordNumber = $"LV-{DateTime.UtcNow.Year}-{(baseNumber + (++i)):D6}",
                EmployeeId = empId, LeaveTypeId = req.LeaveTypeId,
                StartDate = start, EndDate = end, DaysCount = days,
                AffectsBalance = affects, BalanceBefore = before, BalanceAfter = after,
                Status = LeaveRecordStatus.Assigned, Source = LeaveRecordSource.HRAssignment,
                LeaveAssignmentId = assignment.Id, ApprovedByUserId = _user.UserId, ApprovedAt = DateTime.UtcNow,
                Notes = req.Notes, AttachmentUrl = req.AttachmentUrl,
            };
            _db.LeaveRecords.Add(record);

            if (affects) LinkLastTransaction(record.Id);

            if (rules.AffectsAttendance)
                for (var dd = start.Date; dd <= end.Date; dd = dd.AddDays(1))
                    _db.AttendanceRecords.Add(new AttendanceRecord
                    {
                        EmployeeId = empId, Date = DateTime.SpecifyKind(dd, DateTimeKind.Utc),
                        Status = AttendanceStatus.OnLeave, Source = "LeaveAssignment", ReferenceId = record.Id,
                    });

            AddAudit(record.Id, empId, "Assigned", $"تعيين إجازة {leaveItem.NameAr} ({days} يوم)", $"Assigned {leaveItem.NameEn} ({days} days)");
            await _timeline.PublishEvent("Leaves", "LeaveRecord", record.Id, "Assigned", $"Leave assigned ({days} days)", $"تعيين إجازة ({days} يوم)", null, ct);
            await NotifyEmployeeAndManagerAsync(empId, "تم تعيين إجازة", "Leave assigned",
                $"تم تعيين إجازة {leaveItem.NameAr} من {start:yyyy-MM-dd} إلى {end:yyyy-MM-dd}",
                $"{leaveItem.NameEn} leave assigned {start:yyyy-MM-dd} → {end:yyyy-MM-dd}", record.Id, ct);
        }

        await _audit.LogChange("LeaveAssignment", assignment.Id, "Created", null, new { assignment.AssignedCount, leaveItem.Code }, ct);
        await _db.SaveChangesAsync(ct);
        return empIds.Count;
    }

    // ── Edit ──────────────────────────────────────────────────────────────────────

    public async Task EditAsync(Guid id, EditLeaveRequest req, CancellationToken ct)
    {
        var r = await _db.LeaveRecords.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Leave record not found");
        if (r.Status == LeaveRecordStatus.Canceled) throw new InvalidOperationException("لا يمكن تعديل إجازة ملغاة");

        var oldType = r.LeaveTypeId; var oldStart = r.StartDate; var oldEnd = r.EndDate; var oldDays = r.DaysCount; var oldAffects = r.AffectsBalance;

        var newType = req.LeaveTypeId ?? r.LeaveTypeId;
        var newStart = req.StartDate is { } s ? Utc(s) : r.StartDate;
        var newEnd = req.EndDate is { } e ? Utc(e) : r.EndDate;
        if (newEnd < newStart) throw new InvalidOperationException("تاريخ النهاية قبل تاريخ البداية");

        var leaveItem = await _db.MasterDataItems.FirstOrDefaultAsync(m => m.Id == newType, ct)
            ?? throw new InvalidOperationException("نوع الإجازة غير موجود");
        var rules = _leave.GetRules(leaveItem.MetadataJson);
        var newDays = _leave.ComputeDays(newStart, newEnd, rules);
        var newAffects = rules.Paid && newDays > 0;

        // Restore the old deduction, then deduct the new one (handles a leave-type change).
        if (oldAffects) await AdjustBalanceAsync(r.EmployeeId, oldType, oldStart.Year, -oldDays, 30m, "تعديل إجازة - استرجاع", r.Id, ct);
        var before = await RemainingWithDefaultAsync(r.EmployeeId, newType, newStart.Year, (decimal)rules.AnnualBalance, ct);
        var after = before;
        if (newAffects) after = await AdjustBalanceAsync(r.EmployeeId, newType, newStart.Year, newDays, (decimal)rules.AnnualBalance, "تعديل إجازة - خصم", r.Id, ct);

        // Re-mark attendance for the new range.
        await RemoveLeaveAttendanceAsync(r, oldStart, oldEnd, ct);
        if (rules.AffectsAttendance)
            for (var dd = newStart.Date; dd <= newEnd.Date; dd = dd.AddDays(1))
                _db.AttendanceRecords.Add(new AttendanceRecord
                {
                    EmployeeId = r.EmployeeId, Date = DateTime.SpecifyKind(dd, DateTimeKind.Utc),
                    Status = AttendanceStatus.OnLeave,
                    Source = r.Source == LeaveRecordSource.HRAssignment ? "LeaveAssignment" : "LeaveRequest",
                    ReferenceId = r.RequestInstanceId ?? r.Id,
                });

        r.LeaveTypeId = newType; r.StartDate = newStart; r.EndDate = newEnd; r.DaysCount = newDays;
        r.AffectsBalance = newAffects; r.BalanceBefore = before; r.BalanceAfter = after;
        r.Status = LeaveRecordStatus.Edited;
        if (req.Notes is not null) r.Notes = req.Notes;
        if (req.AttachmentUrl is not null) r.AttachmentUrl = req.AttachmentUrl;

        AddAudit(r.Id, r.EmployeeId, "Edited", $"تعديل الإجازة: {newDays} يوم ({newStart:yyyy-MM-dd} → {newEnd:yyyy-MM-dd})", $"Leave edited: {newDays} days");
        await _timeline.PublishEvent("Leaves", "LeaveRecord", r.Id, "Edited", "Leave edited", "تم تعديل الإجازة", null, ct);
        await _audit.LogChange("LeaveRecord", r.Id, "Edited", new { oldDays, oldStart, oldEnd }, new { newDays, newStart, newEnd }, ct);
        await NotifyEmployeeAndManagerAsync(r.EmployeeId, "تم تعديل إجازتك", "Your leave was edited",
            $"تم تعديل الإجازة لتصبح من {newStart:yyyy-MM-dd} إلى {newEnd:yyyy-MM-dd}", $"Leave updated {newStart:yyyy-MM-dd} → {newEnd:yyyy-MM-dd}", r.Id, ct);

        await _db.SaveChangesAsync(ct);
    }

    // ── Cancel ───────────────────────────────────────────────────────────────────

    public async Task CancelAsync(Guid id, string? reason, CancellationToken ct)
    {
        var r = await _db.LeaveRecords.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Leave record not found");
        if (r.Status == LeaveRecordStatus.Canceled) throw new InvalidOperationException("الإجازة ملغاة بالفعل");

        decimal restored = 0m;
        if (r.AffectsBalance && r.DaysCount > 0)
        {
            await AdjustBalanceAsync(r.EmployeeId, r.LeaveTypeId, r.StartDate.Year, -r.DaysCount, 30m, "إلغاء إجازة - استرجاع الرصيد", r.Id, ct);
            restored = r.DaysCount;
            r.BalanceAfter = r.BalanceBefore;
        }

        r.Status = LeaveRecordStatus.Canceled;
        r.CanceledAt = DateTime.UtcNow;
        r.CanceledByUserId = _user.UserId;
        r.CancelReason = reason;

        _db.LeaveCancellations.Add(new LeaveCancellation
        {
            LeaveRecordId = r.Id, Reason = reason, RestoredDays = restored,
            CanceledByUserId = _user.UserId, CanceledAt = DateTime.UtcNow,
        });

        await RemoveLeaveAttendanceAsync(r, r.StartDate, r.EndDate, ct);

        AddAudit(r.Id, r.EmployeeId, "Canceled", $"إلغاء الإجازة" + (restored > 0 ? $" واسترجاع {restored} يوم" : ""), $"Leave canceled (restored {restored})");
        await _timeline.PublishEvent("Leaves", "LeaveRecord", r.Id, "Canceled", "Leave canceled", "تم إلغاء الإجازة", null, ct);
        await _audit.LogChange("LeaveRecord", r.Id, "Canceled", new { r.Status }, new { reason, restored }, ct);
        await NotifyEmployeeAndManagerAsync(r.EmployeeId, "تم إلغاء إجازتك", "Your leave was canceled",
            $"تم إلغاء الإجازة {r.RecordNumber}" + (reason is { Length: > 0 } ? $" — {reason}" : ""), $"Leave {r.RecordNumber} canceled", r.Id, ct);

        await _db.SaveChangesAsync(ct);
    }

    // ── Employee balance ──────────────────────────────────────────────────────────

    public async Task<List<LeaveBalanceDto>> GetEmployeeBalanceAsync(Guid employeeId, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var types = await _db.MasterDataItems.AsNoTracking()
            .Where(m => m.ObjectType == MasterDataObjectType.LeaveType && m.IsActive)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.NameAr).ToListAsync(ct);
        var balances = await _db.LeaveBalances.AsNoTracking()
            .Where(b => b.EmployeeId == employeeId && b.Year == year).ToListAsync(ct);

        return types.Select(t =>
        {
            var rules = _leave.GetRules(t.MetadataJson);
            var bal = balances.FirstOrDefault(b => b.LeaveTypeId == t.Id);
            var entitled = bal?.EntitledDays ?? (decimal)rules.AnnualBalance;
            var carried = bal?.CarriedForwardDays ?? 0m;
            var used = bal?.UsedDays ?? 0m;
            return new LeaveBalanceDto
            {
                LeaveTypeId = t.Id, LeaveTypeName = t.NameAr ?? t.NameEn,
                AffectsBalance = rules.Paid, Entitled = entitled + carried, Used = used, Remaining = entitled + carried - used,
            };
        }).ToList();
    }

    // ── Print (Document Template Engine) ──────────────────────────────────────────

    public async Task<(byte[] pdf, string fileName)> PrintAsync(Guid id, CancellationToken ct)
    {
        var r = await _db.LeaveRecords.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Leave record not found");

        var e = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == r.EmployeeId, ct);
        var company = await _db.CompanyProfiles.AsNoTracking().FirstOrDefaultAsync(ct);
        var lt = await _db.MasterDataItems.AsNoTracking().Where(m => m.Id == r.LeaveTypeId).Select(m => m.NameAr).FirstOrDefaultAsync(ct);
        var dept = e?.DepartmentId is { } dep ? await _db.Departments.AsNoTracking().Where(d => d.Id == dep).Select(d => d.NameAr ?? d.Name).FirstOrDefaultAsync(ct) : null;
        var job = e?.JobTitleId is { } jt ? await _db.Positions.AsNoTracking().Where(p => p.Id == jt).Select(p => p.NameAr ?? p.NameEn).FirstOrDefaultAsync(ct) : null;
        var approver = r.ApprovedByUserId is { } uid ? (await UserNamesAsync(new() { uid }, ct)).GetValueOrDefault(uid) : null;
        var empName = e is null ? "—" : $"{e.FirstNameAr ?? e.FirstName} {e.LastNameAr ?? e.LastName}".Trim();
        string D(DateTime? x) => x?.ToString("yyyy-MM-dd") ?? "";

        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Employee.FullName"] = empName,
            ["Employee.EmployeeNumber"] = e?.EmployeeNumber ?? "",
            ["Employee.Department"] = dept ?? "",
            ["Employee.JobTitle"] = job ?? "",
            ["Company.Name"] = company?.NameAr ?? "",
            ["Company.NameEn"] = company?.NameEn ?? "",
            ["Company.CR"] = company?.CommercialRegistration ?? "",
            ["Company.VAT"] = company?.VatNumber ?? "",
            ["Leave.RecordNumber"] = r.RecordNumber,
            ["Leave.Type"] = lt ?? "",
            ["Leave.StartDate"] = D(r.StartDate),
            ["Leave.EndDate"] = D(r.EndDate),
            ["Leave.Days"] = r.DaysCount.ToString("0.##"),
            ["Leave.BalanceBefore"] = r.AffectsBalance ? r.BalanceBefore.ToString("0.##") : "لا يؤثر على الرصيد",
            ["Leave.BalanceAfter"] = r.AffectsBalance ? r.BalanceAfter.ToString("0.##") : "لا يؤثر على الرصيد",
            ["Leave.DaysDeducted"] = r.AffectsBalance ? r.DaysCount.ToString("0.##") : "0",
            ["Leave.ApprovedDate"] = D(r.ApprovedAt),
            ["Leave.ApprovedBy"] = approver ?? "",
            ["Leave.Notes"] = r.Notes ?? "",
            ["System.Today"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
        };

        var defaultDetails = new List<(string, string)>
        {
            ("اسم الموظف", empName),
            ("الرقم الوظيفي", e?.EmployeeNumber ?? "—"),
            ("الإدارة", dept ?? "—"),
            ("المسمى الوظيفي", job ?? "—"),
            ("نوع الإجازة", lt ?? "—"),
            ("من تاريخ", D(r.StartDate)),
            ("إلى تاريخ", D(r.EndDate)),
            ("عدد الأيام", r.DaysCount.ToString("0.##")),
            ("الرصيد قبل الإجازة", tokens["Leave.BalanceBefore"]),
            ("المخصوم", tokens["Leave.DaysDeducted"]),
            ("الرصيد بعد الاعتماد", tokens["Leave.BalanceAfter"]),
            ("تاريخ الاعتماد", D(r.ApprovedAt)),
            ("اعتمدها", approver ?? "—"),
            ("ملاحظات", r.Notes ?? "—"),
        };

        var templateId = await _db.DocumentTemplates.Where(t => t.Code == "DOC_LEAVE_RECORD").Select(t => (Guid?)t.Id).FirstOrDefaultAsync(ct);

        IReadOnlyList<(int, string, string)>? approvals = null;
        if (r.RequestInstanceId is { } ri)
            approvals = await _db.RequestApprovals.Where(a => a.RequestInstanceId == ri).OrderBy(a => a.StepOrder)
                .Select(a => new ValueTuple<int, string, string>(a.StepOrder, a.StepNameAr, a.Status.ToString())).ToListAsync(ct);

        var render = new DocumentRenderRequest(
            TemplateId: templateId, FallbackTitle: "سجل إجازة", RefNumber: r.RecordNumber,
            Tokens: tokens, DefaultDetails: defaultDetails, Approvals: approvals,
            FileName: $"leave-{r.RecordNumber}.pdf");
        var (pdf, fileName) = await _renderer.RenderDocumentAsync(render, ct);

        // Persist a GeneratedDocument record + audit.
        var doc = new GeneratedDocument
        {
            DocumentTemplateId = templateId ?? Guid.Empty,
            EntityType = "LeaveRecord", EntityId = r.Id,
            Status = DocumentGenerationStatus.Completed, OutputFormat = DocumentOutputFormat.Pdf,
            FileName = fileName, GeneratedAt = DateTime.UtcNow, GeneratedById = _user.UserId,
        };
        if (templateId is not null) { _db.Set<GeneratedDocument>().Add(doc); r.GeneratedDocumentId = doc.Id; }
        AddAudit(r.Id, r.EmployeeId, "Printed", "طباعة سجل الإجازة", "Leave record printed");
        await _db.SaveChangesAsync(ct);

        return (pdf, fileName);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private async Task<List<Guid>> ResolveTargetEmployeesAsync(AssignLeaveRequest req, CancellationToken ct)
    {
        var active = _db.Employees.AsNoTracking().Where(e => e.Status == EmployeeStatus.Active);
        return req.Scope switch
        {
            "Department" when req.DepartmentId is { } d => await active.Where(e => e.DepartmentId == d).Select(e => e.Id).ToListAsync(ct),
            "Branch" when req.BranchId is { } b => await active.Where(e => e.BranchId == b).Select(e => e.Id).ToListAsync(ct),
            "JobTitle" when req.JobTitleId is { } j => await active.Where(e => e.JobTitleId == j).Select(e => e.Id).ToListAsync(ct),
            _ => req.EmployeeIds.Distinct().ToList(),
        };
    }

    private async Task<decimal> RemainingAsync(Guid employeeId, Guid leaveTypeId, int year, CancellationToken ct)
    {
        var bal = await _db.LeaveBalances.FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.Year == year, ct);
        if (bal is null) return 0m;
        return bal.EntitledDays + bal.CarriedForwardDays - bal.UsedDays;
    }

    /// <summary>Remaining balance, falling back to the leave type's default annual entitlement when the
    /// employee has no balance row yet (so "balance before" is the real entitlement, not 0).</summary>
    private async Task<decimal> RemainingWithDefaultAsync(Guid employeeId, Guid leaveTypeId, int year, decimal defaultEntitled, CancellationToken ct)
    {
        var bal = await _db.LeaveBalances.FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.Year == year, ct);
        var entitled = bal?.EntitledDays ?? defaultEntitled;
        var carried = bal?.CarriedForwardDays ?? 0m;
        var used = bal?.UsedDays ?? 0m;
        return entitled + carried - used;
    }

    /// <summary>Apply a change of <paramref name="deltaDays"/> used-days (positive = deduct, negative =
    /// restore), creating the balance row if needed, writing a ledger entry, and returning the new
    /// remaining balance.</summary>
    private async Task<decimal> AdjustBalanceAsync(Guid employeeId, Guid leaveTypeId, int year, decimal deltaDays, decimal entitledDefault, string reason, Guid? recordId, CancellationToken ct)
    {
        var bal = await _db.LeaveBalances.FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.Year == year, ct);
        if (bal is null)
        {
            bal = new LeaveBalance { EmployeeId = employeeId, LeaveTypeId = leaveTypeId, Year = year, EntitledDays = entitledDefault, UsedDays = 0m };
            _db.LeaveBalances.Add(bal);
        }
        bal.UsedDays += deltaDays;
        if (bal.UsedDays < 0) bal.UsedDays = 0;
        var remaining = bal.EntitledDays + bal.CarriedForwardDays - bal.UsedDays;
        _db.LeaveBalanceTransactions.Add(new LeaveBalanceTransaction
        {
            EmployeeId = employeeId, LeaveTypeId = leaveTypeId, Year = year, LeaveRecordId = recordId,
            Delta = -deltaDays, BalanceAfter = remaining, Reason = reason, ActorUserId = _user.UserId, At = DateTime.UtcNow,
        });
        return remaining;
    }

    /// <summary>After AdjustBalance added a tracked transaction with a null record id (assign path
    /// creates the record after), back-fill the record id on the most recent pending transaction.</summary>
    private void LinkLastTransaction(Guid recordId)
    {
        var txn = _db.ChangeTracker.Entries<LeaveBalanceTransaction>()
            .Where(e => e.State == EntityState.Added && e.Entity.LeaveRecordId == null)
            .Select(e => e.Entity).LastOrDefault();
        if (txn is not null) txn.LeaveRecordId = recordId;
    }

    private async Task RemoveLeaveAttendanceAsync(LeaveRecord r, DateTime start, DateTime end, CancellationToken ct)
    {
        var refIds = new List<Guid> { r.Id };
        if (r.RequestInstanceId is { } ri) refIds.Add(ri);
        var rows = await _db.AttendanceRecords
            .Where(a => a.EmployeeId == r.EmployeeId && a.Date >= start && a.Date <= end
                        && a.Status == AttendanceStatus.OnLeave && a.ReferenceId != null && refIds.Contains(a.ReferenceId.Value))
            .ToListAsync(ct);
        foreach (var a in rows) a.IsDeleted = true;
    }

    private void AddAudit(Guid recordId, Guid employeeId, string action, string ar, string en)
        => _db.LeaveAuditLogs.Add(new LeaveAuditLog
        {
            LeaveRecordId = recordId, EmployeeId = employeeId, Action = action,
            DetailsAr = ar, DetailsEn = en, ActorUserId = _user.UserId, At = DateTime.UtcNow,
        });

    private async Task NotifyEmployeeAndManagerAsync(Guid employeeId, string titleAr, string titleEn, string bodyAr, string bodyEn, Guid recordId, CancellationToken ct)
    {
        var emp = await _db.Employees.Where(e => e.Id == employeeId).Select(e => new { e.UserId, e.ManagerId }).FirstOrDefaultAsync(ct);
        if (emp?.UserId is { } uid)
            await _notify.NotifyAsync(uid, titleAr, titleEn, bodyAr, bodyEn, "Leave", recordId, "/leaves", ct: ct);
        if (emp?.ManagerId is { } mgr)
        {
            var mgrUser = await _db.Employees.Where(e => e.Id == mgr).Select(e => e.UserId).FirstOrDefaultAsync(ct);
            if (mgrUser is { } muid) await _notify.NotifyAsync(muid, titleAr, titleEn, bodyAr, bodyEn, "Leave", recordId, "/leaves", ct: ct);
        }
    }

    private async Task<Dictionary<Guid, string>> UserNamesAsync(List<Guid> userIds, CancellationToken ct)
    {
        if (userIds.Count == 0) return new();
        var byEmployee = await _db.Employees.AsNoTracking().Where(e => e.UserId != null && userIds.Contains(e.UserId!.Value))
            .Select(e => new { e.UserId, Name = (e.FirstNameAr ?? e.FirstName) + " " + (e.LastNameAr ?? e.LastName) })
            .ToListAsync(ct);
        var map = byEmployee.Where(x => x.UserId != null).ToDictionary(x => x.UserId!.Value, x => x.Name);
        var missing = userIds.Where(id => !map.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            var users = await _db.Users.AsNoTracking().Where(u => missing.Contains(u.Id)).Select(u => new { u.Id, u.Email }).ToListAsync(ct);
            foreach (var u in users) map[u.Id] = u.Email;
        }
        return map;
    }

    private async Task BackfillFromRequestsAsync(CancellationToken ct)
    {
        var linked = await _db.LeaveRecords.AsNoTracking().Where(r => r.RequestInstanceId != null)
            .Select(r => r.RequestInstanceId!.Value).ToListAsync(ct);

        var pending = await _db.RequestInstances.AsNoTracking()
            .Where(r => r.LeaveTypeId != null && r.Status == RequestStatus.Approved && !linked.Contains(r.Id))
            .ToListAsync(ct);
        if (pending.Count == 0) return;

        var baseNumber = await _db.LeaveRecords.CountAsync(ct);
        var i = 0;
        foreach (var ins in pending)
        {
            var lt = await _db.MasterDataItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == ins.LeaveTypeId!.Value, ct);
            var rules = _leave.GetRules(lt?.MetadataJson);
            var days = ins.DaysCount ?? 0m;
            var affects = rules.Paid && days > 0;
            var remainingNow = await RemainingAsync(ins.EmployeeId, ins.LeaveTypeId!.Value, (ins.StartDate ?? ins.SubmittedAt).Year, ct);
            _db.LeaveRecords.Add(new LeaveRecord
            {
                RecordNumber = $"LV-{(ins.StartDate ?? ins.SubmittedAt).Year}-{(baseNumber + (++i)):D6}",
                EmployeeId = ins.EmployeeId, LeaveTypeId = ins.LeaveTypeId!.Value,
                StartDate = ins.StartDate ?? ins.SubmittedAt, EndDate = ins.EndDate ?? ins.StartDate ?? ins.SubmittedAt,
                DaysCount = days, AffectsBalance = affects,
                BalanceBefore = affects ? remainingNow + days : remainingNow, BalanceAfter = remainingNow,
                Status = LeaveRecordStatus.Approved, Source = LeaveRecordSource.Request,
                RequestInstanceId = ins.Id, ApprovedByUserId = null, ApprovedAt = ins.DecidedAt,
                Notes = ins.DecisionNote,
            });
        }
        await _db.SaveChangesAsync(ct);
    }
}

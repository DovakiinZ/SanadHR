using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Domain.Engines.Attendance;
using HR.Infrastructure.Persistence;
using HR.Modules.Attendance.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Attendance.Controllers;

/// <summary>Shift templates + assignment engine. HR/Admin create shifts and assign them by employee /
/// department / branch / job-title over an effective date range.</summary>
[Authorize]
[Route("api/shifts")]
public class ShiftsController : BaseApiController
{
    private readonly ApplicationDbContext _db;
    public ShiftsController(ApplicationDbContext db) { _db = db; }

    private static string Hm(TimeOnly t) => t.ToString("HH:mm");
    private static TimeOnly ParseHm(string? s, TimeOnly fallback)
        => TimeOnly.TryParse(s, out var t) ? t : fallback;

    // ── Shifts CRUD ───────────────────────────────────────────────────────────

    [HttpGet]
    [RequirePermission("Attendance.View")]
    public async Task<ActionResult<ApiResponse<List<ShiftDto>>>> List(CancellationToken ct)
    {
        var counts = await _db.ShiftAssignments.Where(a => a.IsActive)
            .GroupBy(a => a.ShiftId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        var shifts = await _db.Shifts.AsNoTracking().OrderBy(s => s.NameAr).ToListAsync(ct);
        var dtos = shifts.Select(s => Map(s, counts.TryGetValue(s.Id, out var c) ? c : 0)).ToList();
        return OkResponse(dtos);
    }

    [HttpPost]
    [RequirePermission("Attendance.Edit")]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> Create([FromBody] ShiftInput input, CancellationToken ct)
    {
        var shift = new Shift();
        Apply(shift, input);
        _db.Shifts.Add(shift);
        await _db.SaveChangesAsync(ct);
        return CreatedResponse(Map(shift, 0));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Attendance.Edit")]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> Update(Guid id, [FromBody] ShiftInput input, CancellationToken ct)
    {
        var shift = await _db.Shifts.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (shift is null) return NotFound(ApiResponse<ShiftDto>.Fail("Shift not found"));
        Apply(shift, input);
        await _db.SaveChangesAsync(ct);
        return OkResponse(Map(shift, 0));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Attendance.Edit")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var shift = await _db.Shifts.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (shift is null) return NotFound(ApiResponse.Fail("Shift not found"));
        shift.IsDeleted = true;
        // Deactivate its assignments so resolution ignores them.
        var assigns = await _db.ShiftAssignments.Where(a => a.ShiftId == id).ToListAsync(ct);
        foreach (var a in assigns) a.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return OkResponse("تم حذف الوردية");
    }

    // ── Assignments ─────────────────────────────────────────────────────────────

    [HttpGet("assignments")]
    [RequirePermission("Attendance.View")]
    public async Task<ActionResult<ApiResponse<List<ShiftAssignmentDto>>>> Assignments([FromQuery] Guid? shiftId, CancellationToken ct)
    {
        var q = _db.ShiftAssignments.AsNoTracking().AsQueryable();
        if (shiftId is { } sid) q = q.Where(a => a.ShiftId == sid);

        var rows = await (from a in q
                          join s in _db.Shifts on a.ShiftId equals s.Id into sj
                          from s in sj.DefaultIfEmpty()
                          join e in _db.Employees on a.EmployeeId equals e.Id into ej
                          from e in ej.DefaultIfEmpty()
                          join d in _db.Departments on a.DepartmentId equals d.Id into dj
                          from d in dj.DefaultIfEmpty()
                          join b in _db.Branch on a.BranchId equals b.Id into bj
                          from b in bj.DefaultIfEmpty()
                          join p in _db.Positions on a.JobTitleId equals p.Id into pj
                          from p in pj.DefaultIfEmpty()
                          orderby a.CreatedAt descending
                          select new ShiftAssignmentDto
                          {
                              Id = a.Id, ShiftId = a.ShiftId, ShiftName = s != null ? (s.NameAr ?? s.NameEn) : null,
                              EmployeeId = a.EmployeeId,
                              EmployeeName = e != null ? ((e.FirstNameAr ?? e.FirstName) + " " + (e.LastNameAr ?? e.LastName)) : null,
                              DepartmentId = a.DepartmentId, DepartmentName = d != null ? (d.NameAr ?? d.Name) : null,
                              BranchId = a.BranchId, BranchName = b != null ? (b.NameAr ?? b.Name) : null,
                              JobTitleId = a.JobTitleId, JobTitleName = p != null ? (p.NameAr ?? p.NameEn) : null,
                              EffectiveFrom = a.EffectiveFrom, EffectiveTo = a.EffectiveTo,
                              Priority = a.Priority, IsActive = a.IsActive,
                          }).ToListAsync(ct);
        return OkResponse(rows);
    }

    [HttpPost("assign")]
    [RequirePermission("Attendance.Edit")]
    public async Task<ActionResult<ApiResponse<int>>> Assign([FromBody] AssignShiftRequest req, CancellationToken ct)
    {
        if (!await _db.Shifts.AnyAsync(s => s.Id == req.ShiftId, ct))
            return NotFound(ApiResponse<int>.Fail("Shift not found"));

        var from = DateTime.SpecifyKind(req.EffectiveFrom.Date, DateTimeKind.Utc);
        DateTime? to = req.EffectiveTo is { } t ? DateTime.SpecifyKind(t.Date, DateTimeKind.Utc) : null;
        var created = 0;

        ShiftAssignment New(Guid? emp, Guid? dep, Guid? br, Guid? jt) => new()
        {
            ShiftId = req.ShiftId, EmployeeId = emp, DepartmentId = dep, BranchId = br, JobTitleId = jt,
            EffectiveFrom = from, EffectiveTo = to, Priority = req.Priority, IsActive = req.IsActive,
        };

        foreach (var empId in req.EmployeeIds.Distinct())
        {
            _db.ShiftAssignments.Add(New(empId, null, null, null));
            created++;
        }
        if (req.DepartmentId is { } d) { _db.ShiftAssignments.Add(New(null, d, null, null)); created++; }
        if (req.BranchId is { } b) { _db.ShiftAssignments.Add(New(null, null, b, null)); created++; }
        if (req.JobTitleId is { } j) { _db.ShiftAssignments.Add(New(null, null, null, j)); created++; }

        if (created == 0)
            return BadRequest(ApiResponse<int>.Fail("حدد موظفاً أو إدارة أو فرعاً أو مسمى وظيفياً على الأقل"));

        await _db.SaveChangesAsync(ct);
        return CreatedResponse(created, $"تم إنشاء {created} تعيين");
    }

    [HttpDelete("assignments/{id:guid}")]
    [RequirePermission("Attendance.Edit")]
    public async Task<ActionResult<ApiResponse>> DeleteAssignment(Guid id, CancellationToken ct)
    {
        var a = await _db.ShiftAssignments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a is null) return NotFound(ApiResponse.Fail("Assignment not found"));
        a.IsDeleted = true;
        a.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return OkResponse("تم حذف التعيين");
    }

    // ── mapping ──
    private static void Apply(Shift s, ShiftInput i)
    {
        s.NameAr = i.NameAr; s.NameEn = i.NameEn;
        s.StartTime = ParseHm(i.StartTime, new TimeOnly(8, 0));
        s.EndTime = ParseHm(i.EndTime, new TimeOnly(17, 0));
        s.RequiredMinutes = i.RequiredMinutes;
        s.BreakMinutes = i.BreakMinutes;
        s.GraceBeforeStartMinutes = i.GraceBeforeStartMinutes;
        s.GraceAfterStartMinutes = i.GraceAfterStartMinutes;
        s.GraceBeforeEndMinutes = i.GraceBeforeEndMinutes;
        s.GraceAfterEndMinutes = i.GraceAfterEndMinutes;
        s.OvertimeAllowed = i.OvertimeAllowed;
        s.LateDeductionEnabled = i.LateDeductionEnabled;
        s.IsFlexible = i.IsFlexible;
        s.WeekendDays = string.IsNullOrWhiteSpace(i.WeekendDays) ? "5,6" : i.WeekendDays;
        s.IsActive = i.IsActive;
    }

    private static ShiftDto Map(Shift s, int assignedCount) => new()
    {
        Id = s.Id, NameAr = s.NameAr, NameEn = s.NameEn,
        StartTime = Hm(s.StartTime), EndTime = Hm(s.EndTime),
        RequiredMinutes = s.RequiredMinutes, BreakMinutes = s.BreakMinutes,
        GraceBeforeStartMinutes = s.GraceBeforeStartMinutes, GraceAfterStartMinutes = s.GraceAfterStartMinutes,
        GraceBeforeEndMinutes = s.GraceBeforeEndMinutes, GraceAfterEndMinutes = s.GraceAfterEndMinutes,
        OvertimeAllowed = s.OvertimeAllowed, LateDeductionEnabled = s.LateDeductionEnabled,
        IsFlexible = s.IsFlexible, WeekendDays = s.WeekendDays, IsActive = s.IsActive,
        AssignedCount = assignedCount,
    };
}

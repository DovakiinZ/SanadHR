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

/// <summary>Tenant attendance settings: official holiday calendar + the attendance policy
/// (default grace, worked-minute rounding, auto-absent, overtime gate).</summary>
[Authorize]
public class AttendanceSettingsController : BaseApiController
{
    private readonly ApplicationDbContext _db;
    public AttendanceSettingsController(ApplicationDbContext db) { _db = db; }

    private static DateTime Utc(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
    private static AttendanceHolidayDto Map(AttendanceHoliday h) => new()
    {
        Id = h.Id, NameAr = h.NameAr, NameEn = h.NameEn, Date = h.Date.ToString("yyyy-MM-dd"),
        IsRecurring = h.IsRecurring, IsActive = h.IsActive,
    };

    // ── Holidays ────────────────────────────────────────────────────────────────

    [HttpGet("/api/attendance/holidays")]
    [RequirePermission("Attendance.View")]
    public async Task<ActionResult<ApiResponse<List<AttendanceHolidayDto>>>> Holidays(CancellationToken ct)
    {
        var rows = await _db.AttendanceHolidays.AsNoTracking().OrderBy(h => h.Date).ToListAsync(ct);
        return OkResponse(rows.Select(Map).ToList());
    }

    [HttpPost("/api/attendance/holidays")]
    [RequirePermission("Attendance.Edit")]
    public async Task<ActionResult<ApiResponse<AttendanceHolidayDto>>> CreateHoliday([FromBody] HolidayInput input, CancellationToken ct)
    {
        var h = new AttendanceHoliday
        {
            NameAr = input.NameAr, NameEn = string.IsNullOrWhiteSpace(input.NameEn) ? input.NameAr : input.NameEn,
            Date = Utc(input.Date), IsRecurring = input.IsRecurring, IsActive = input.IsActive,
        };
        _db.AttendanceHolidays.Add(h);
        await _db.SaveChangesAsync(ct);
        return CreatedResponse(Map(h));
    }

    [HttpPut("/api/attendance/holidays/{id:guid}")]
    [RequirePermission("Attendance.Edit")]
    public async Task<ActionResult<ApiResponse<AttendanceHolidayDto>>> UpdateHoliday(Guid id, [FromBody] HolidayInput input, CancellationToken ct)
    {
        var h = await _db.AttendanceHolidays.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (h is null) return NotFound(ApiResponse<AttendanceHolidayDto>.Fail("Holiday not found"));
        h.NameAr = input.NameAr;
        h.NameEn = string.IsNullOrWhiteSpace(input.NameEn) ? input.NameAr : input.NameEn;
        h.Date = Utc(input.Date);
        h.IsRecurring = input.IsRecurring;
        h.IsActive = input.IsActive;
        await _db.SaveChangesAsync(ct);
        return OkResponse(Map(h));
    }

    [HttpDelete("/api/attendance/holidays/{id:guid}")]
    [RequirePermission("Attendance.Edit")]
    public async Task<ActionResult<ApiResponse>> DeleteHoliday(Guid id, CancellationToken ct)
    {
        var h = await _db.AttendanceHolidays.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (h is null) return NotFound(ApiResponse.Fail("Holiday not found"));
        h.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return OkResponse("تم حذف العطلة");
    }

    // ── Policy (single default per tenant) ──────────────────────────────────────

    [HttpGet("/api/attendance/policy")]
    [RequirePermission("Attendance.View")]
    public async Task<ActionResult<ApiResponse<AttendancePolicyDto>>> GetPolicy(CancellationToken ct)
    {
        var p = await _db.AttendancePolicies.AsNoTracking().OrderByDescending(x => x.IsDefault).FirstOrDefaultAsync(ct);
        return OkResponse(p is null ? new AttendancePolicyDto() : new AttendancePolicyDto
        {
            Id = p.Id, NameAr = p.NameAr, NameEn = p.NameEn,
            DefaultGraceMinutes = p.DefaultGraceMinutes, RoundingMinutes = p.RoundingMinutes,
            AutoMarkAbsent = p.AutoMarkAbsent, CountOvertime = p.CountOvertime, IsActive = p.IsActive,
        });
    }

    [HttpPut("/api/attendance/policy")]
    [RequirePermission("Attendance.Edit")]
    public async Task<ActionResult<ApiResponse<AttendancePolicyDto>>> UpdatePolicy([FromBody] PolicyInput input, CancellationToken ct)
    {
        var p = await _db.AttendancePolicies.OrderByDescending(x => x.IsDefault).FirstOrDefaultAsync(ct);
        if (p is null)
        {
            p = new AttendancePolicy { IsDefault = true, IsActive = true };
            _db.AttendancePolicies.Add(p);
        }
        p.DefaultGraceMinutes = Math.Max(0, input.DefaultGraceMinutes);
        p.RoundingMinutes = Math.Max(0, input.RoundingMinutes);
        p.AutoMarkAbsent = input.AutoMarkAbsent;
        p.CountOvertime = input.CountOvertime;
        await _db.SaveChangesAsync(ct);
        return OkResponse(new AttendancePolicyDto
        {
            Id = p.Id, NameAr = p.NameAr, NameEn = p.NameEn,
            DefaultGraceMinutes = p.DefaultGraceMinutes, RoundingMinutes = p.RoundingMinutes,
            AutoMarkAbsent = p.AutoMarkAbsent, CountOvertime = p.CountOvertime, IsActive = p.IsActive,
        });
    }
}

using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Modules.Attendance.DTOs;
using HR.Modules.Attendance.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Modules.Attendance.Controllers;

/// <summary>Full daily/weekly/monthly attendance engine for all employees. Rows are computed
/// server-side from live punches + the resolved shift; approved leave / missing-punch / correction
/// requests are overlaid. See <see cref="IAttendanceService"/>.</summary>
[Authorize]
[Route("api/attendance")]
public class AttendanceController : BaseApiController
{
    private readonly IAttendanceService _svc;
    public AttendanceController(IAttendanceService svc) { _svc = svc; }

    public sealed class AttendanceQuery
    {
        public DateTime? Date { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? JobTitleId { get; set; }
        public Guid? ShiftId { get; set; }
        public string? Status { get; set; }

        public AttendanceFilter ToFilter() => new()
        {
            EmployeeId = EmployeeId, DepartmentId = DepartmentId, BranchId = BranchId,
            JobTitleId = JobTitleId, ShiftId = ShiftId, Status = Status,
        };
    }

    private static DateTime Utc(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
    private static DateTime Today => DateTime.UtcNow.Date;

    /// <summary>Generic list — defaults to today; pass from/to for a range.</summary>
    [HttpGet]
    [RequirePermission("Attendance.View")]
    public async Task<ActionResult<ApiResponse<AttendanceDailyResponse>>> Get([FromQuery] AttendanceQuery q, CancellationToken ct)
    {
        if (q.From is { } f && q.To is { } t)
        {
            var rows = await _svc.GetRangeRowsAsync(q.ToFilter(), Utc(f), Utc(t), ct);
            return OkResponse(new AttendanceDailyResponse { Date = Utc(f), Rows = rows });
        }
        var date = q.Date is { } d ? Utc(d) : Today;
        return OkResponse(await _svc.GetDailyAsync(q.ToFilter(), date, ct));
    }

    [HttpGet("daily")]
    [RequirePermission("Attendance.View")]
    public async Task<ActionResult<ApiResponse<AttendanceDailyResponse>>> Daily([FromQuery] AttendanceQuery q, CancellationToken ct)
    {
        var date = q.Date is { } d ? Utc(d) : Today;
        return OkResponse(await _svc.GetDailyAsync(q.ToFilter(), date, ct));
    }

    [HttpGet("weekly")]
    [RequirePermission("Attendance.View")]
    public async Task<ActionResult<ApiResponse<AttendanceSummaryResponse>>> Weekly([FromQuery] AttendanceQuery q, CancellationToken ct)
    {
        var (from, to) = WeekRange(q);
        return OkResponse(await _svc.GetSummaryAsync(q.ToFilter(), from, to, ct));
    }

    [HttpGet("monthly")]
    [RequirePermission("Attendance.View")]
    public async Task<ActionResult<ApiResponse<AttendanceSummaryResponse>>> Monthly([FromQuery] AttendanceQuery q, CancellationToken ct)
    {
        var (from, to) = MonthRange(q);
        return OkResponse(await _svc.GetSummaryAsync(q.ToFilter(), from, to, ct));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Attendance.View")]
    public async Task<ActionResult<ApiResponse<AttendanceDetailDto>>> GetById(Guid id, CancellationToken ct)
    {
        var detail = await _svc.GetDetailAsync(id, ct);
        if (detail is null) return NotFound(ApiResponse<AttendanceDetailDto>.Fail("Attendance record not found"));
        return OkResponse(detail);
    }

    [HttpPost("manual-punch")]
    [RequirePermission("Attendance.Edit")]
    public async Task<ActionResult<ApiResponse<Guid>>> ManualPunch([FromBody] ManualPunchRequest req, CancellationToken ct)
    {
        var id = await _svc.AddManualPunchAsync(req, ct);
        return CreatedResponse(id, "تم تسجيل البصمة اليدوية");
    }

    [HttpPut("{id:guid}/correct")]
    [RequirePermission("Attendance.Edit")]
    public async Task<ActionResult<ApiResponse>> Correct(Guid id, [FromBody] CorrectAttendanceRequest req, CancellationToken ct)
    {
        await _svc.CorrectAsync(id, req, ct);
        return OkResponse("تم تصحيح الحضور");
    }

    /// <summary>Excel export. view = daily | range | summary (weekly/monthly).</summary>
    [HttpGet("export")]
    [RequirePermission("Attendance.Export")]
    public async Task<IActionResult> Export([FromQuery] AttendanceQuery q, [FromQuery] string view = "daily", CancellationToken ct = default)
    {
        const string mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd");

        if (view is "weekly" or "monthly" or "summary")
        {
            var (from, to) = view == "monthly" ? MonthRange(q) : view == "weekly" ? WeekRange(q) : (q.From is { } f ? Utc(f) : Today, q.To is { } t ? Utc(t) : Today);
            var summary = await _svc.GetSummaryAsync(q.ToFilter(), from, to, ct);
            return File(AttendanceExporter.ExportSummary(summary.Rows), mime, $"attendance-summary-{stamp}.xlsx");
        }

        DateTime rFrom, rTo;
        if (q.From is { } qf && q.To is { } qt) { rFrom = Utc(qf); rTo = Utc(qt); }
        else { rFrom = rTo = q.Date is { } d ? Utc(d) : Today; }
        var rows = await _svc.GetRangeRowsAsync(q.ToFilter(), rFrom, rTo, ct);
        return File(AttendanceExporter.ExportRows(rows), mime, $"attendance-{stamp}.xlsx");
    }

    // ── range helpers ──
    private static (DateTime from, DateTime to) WeekRange(AttendanceQuery q)
    {
        if (q.From is { } f && q.To is { } t) return (Utc(f), Utc(t));
        var anchor = q.Date is { } d ? Utc(d) : Today;
        var start = anchor.AddDays(-(int)anchor.DayOfWeek); // week starts Sunday
        return (start, start.AddDays(6));
    }

    private static (DateTime from, DateTime to) MonthRange(AttendanceQuery q)
    {
        var anchor = q.Date is { } d ? d : Today;
        var year = q.Year ?? anchor.Year;
        var month = q.Month ?? anchor.Month;
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        return (start, start.AddMonths(1).AddDays(-1));
    }
}

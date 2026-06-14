using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Attendance.Controllers;

/// <summary>Attendance records — written by approved Leave / Missing-Punch / Attendance-Correction
/// requests (and later by the attendance module itself).</summary>
[Authorize]
[ApiController]
[Route("api/attendance")]
public class AttendanceController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    public AttendanceController(ApplicationDbContext db, ICurrentUserService user) { _db = db; _user = user; }

    public sealed class AttendanceDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string? Source { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>List attendance records (filter by employee/month). Own records unless the caller
    /// has a view permission or passes a specific employeeId they can see.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AttendanceDto>>>> Get(
        [FromQuery] Guid? employeeId, [FromQuery] int? year, [FromQuery] int? month, [FromQuery] string? scope, CancellationToken ct)
    {
        var canSeeAll = _user.Permissions.Contains("Attendance.View") || _user.Permissions.Contains("Employees.View");
        var q = _db.AttendanceRecords.AsNoTracking().AsQueryable();

        if (employeeId is { } eid && canSeeAll) q = q.Where(a => a.EmployeeId == eid);
        else if (scope == "all" && canSeeAll) { /* all */ }
        else
        {
            var myId = await _db.Employees.Where(e => e.UserId == _user.UserId).Select(e => (Guid?)e.Id).FirstOrDefaultAsync(ct);
            q = q.Where(a => a.EmployeeId == myId);
        }
        if (year is { } y) q = q.Where(a => a.Date.Year == y);
        if (month is { } m) q = q.Where(a => a.Date.Month == m);

        var rows = await (from a in q
                          join emp in _db.Employees on a.EmployeeId equals emp.Id into ej
                          from emp in ej.DefaultIfEmpty()
                          orderby a.Date descending
                          select new AttendanceDto
                          {
                              Id = a.Id, EmployeeId = a.EmployeeId,
                              EmployeeName = emp != null ? ((emp.FirstNameAr ?? emp.FirstName) + " " + (emp.LastNameAr ?? emp.LastName)) : null,
                              Date = a.Date, Status = a.Status.ToString(), CheckIn = a.CheckIn, CheckOut = a.CheckOut,
                              Source = a.Source, Notes = a.Notes,
                          }).Take(500).ToListAsync(ct);
        return Ok(ApiResponse<List<AttendanceDto>>.Ok(rows));
    }
}

using HR.Application.Engines.Completion;
using HR.Domain.Engines.Attendance;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Attendance.Completion;

/// <summary>Effect: mark each day of an approved leave as <see cref="AttendanceStatus.OnLeave"/>.</summary>
public sealed class AttendanceApplyLeaveDaysExecutor : IEffectExecutor
{
    private readonly ApplicationDbContext _db;

    public AttendanceApplyLeaveDaysExecutor(ApplicationDbContext db) => _db = db;

    public string EffectType => EffectTypes.AttendanceApplyLeaveDays;

    public async Task<EffectExecutionResult> ExecuteAsync(EffectContext ctx, CancellationToken ct)
    {
        var start = ctx.Date("startDate") ?? throw new InvalidOperationException("startDate missing.");
        var end = ctx.Date("endDate") ?? throw new InvalidOperationException("endDate missing.");

        var count = 0;
        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
        {
            var day = DateTime.SpecifyKind(d, DateTimeKind.Utc);
            var existing = await _db.AttendanceRecords.FirstOrDefaultAsync(
                a => a.EmployeeId == ctx.EmployeeId && a.Date == day, ct);
            if (existing is null)
            {
                _db.AttendanceRecords.Add(new AttendanceRecord
                {
                    EmployeeId = ctx.EmployeeId, Date = day, Status = AttendanceStatus.OnLeave,
                    Source = "LeaveRequest", ReferenceId = ctx.RequestInstanceId,
                });
            }
            else
            {
                existing.Status = AttendanceStatus.OnLeave;
                existing.Source = "LeaveRequest";
                existing.ReferenceId = ctx.RequestInstanceId;
                existing.LateMinutes = 0;
                existing.ShortageMinutes = 0;
            }
            count++;
        }

        return EffectExecutionResult.Ok(
            targetEntityType: "AttendanceRecord",
            after: new { onLeaveDays = count },
            summary: $"Marked {count} day(s) OnLeave");
    }
}

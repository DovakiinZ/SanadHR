using HR.Application.Engines.Completion;
using HR.Domain.Engines.Attendance;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;

namespace HR.Modules.Attendance.Completion;

/// <summary>Effect: mark each day of an approved leave as <see cref="AttendanceStatus.OnLeave"/>.</summary>
public sealed class AttendanceApplyLeaveDaysExecutor : IEffectExecutor
{
    private readonly ApplicationDbContext _db;

    public AttendanceApplyLeaveDaysExecutor(ApplicationDbContext db) => _db = db;

    public string EffectType => EffectTypes.AttendanceApplyLeaveDays;

    public Task<EffectExecutionResult> ExecuteAsync(EffectContext ctx, CancellationToken ct)
    {
        var start = ctx.Date("startDate") ?? throw new InvalidOperationException("startDate missing.");
        var end = ctx.Date("endDate") ?? throw new InvalidOperationException("endDate missing.");

        var count = 0;
        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
        {
            _db.AttendanceRecords.Add(new AttendanceRecord
            {
                EmployeeId = ctx.EmployeeId,
                Date = DateTime.SpecifyKind(d, DateTimeKind.Utc),
                Status = AttendanceStatus.OnLeave,
                Source = "LeaveRequest",
                ReferenceId = ctx.RequestInstanceId,
            });
            count++;
        }

        return Task.FromResult(EffectExecutionResult.Ok(
            targetEntityType: "AttendanceRecord",
            after: new { onLeaveDays = count },
            summary: $"Marked {count} day(s) OnLeave"));
    }
}

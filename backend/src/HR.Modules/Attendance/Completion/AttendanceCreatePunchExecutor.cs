using HR.Application.Engines.Completion;
using HR.Domain.Engines.Attendance;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;

namespace HR.Modules.Attendance.Completion;

/// <summary>Effect: create an attendance record with check-in/out for an approved missing-punch request.</summary>
public sealed class AttendanceCreatePunchExecutor : IEffectExecutor
{
    private readonly ApplicationDbContext _db;

    public AttendanceCreatePunchExecutor(ApplicationDbContext db) => _db = db;

    public string EffectType => EffectTypes.AttendanceCreatePunch;

    public Task<EffectExecutionResult> ExecuteAsync(EffectContext ctx, CancellationToken ct)
    {
        var date = (ctx.Date("date") ?? DateTime.UtcNow).Date;
        var record = new AttendanceRecord
        {
            EmployeeId = ctx.EmployeeId,
            Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
            Status = AttendanceStatus.Present,
            Source = "MissingPunch",
            ReferenceId = ctx.RequestInstanceId,
            CheckIn = CombineDateTime(date, ctx.Str("checkIn")),
            CheckOut = CombineDateTime(date, ctx.Str("checkOut")),
            Notes = ctx.Str("reason"),
        };
        _db.AttendanceRecords.Add(record);

        return Task.FromResult(EffectExecutionResult.Ok(
            targetEntityType: "AttendanceRecord",
            targetRecordId: record.Id,
            after: new { record.Date, record.CheckIn, record.CheckOut },
            summary: $"Missing punch recorded for {date:yyyy-MM-dd}"));
    }

    /// <summary>Combine a date with an "HH:mm" time string into a UTC DateTime (null when no time).</summary>
    private static DateTime? CombineDateTime(DateTime date, string? time)
    {
        if (string.IsNullOrWhiteSpace(time)) return null;
        return TimeSpan.TryParse(time, out var t)
            ? DateTime.SpecifyKind(date.Date.Add(t), DateTimeKind.Utc)
            : null;
    }
}

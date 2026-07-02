using HR.Application.Engines.Completion;
using HR.Domain.Engines.Attendance;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Attendance.Completion;

/// <summary>Effect: upsert the day's attendance record for an approved (non-leave) correction.</summary>
public sealed class AttendanceCorrectionExecutor : IEffectExecutor
{
    private readonly ApplicationDbContext _db;

    public AttendanceCorrectionExecutor(ApplicationDbContext db) => _db = db;

    public string EffectType => EffectTypes.AttendanceCorrect;

    public async Task<EffectExecutionResult> ExecuteAsync(EffectContext ctx, CancellationToken ct)
    {
        var date = DateTime.SpecifyKind((ctx.Date("date") ?? DateTime.UtcNow).Date, DateTimeKind.Utc);
        var reason = ctx.Str("reason");

        var existing = await _db.AttendanceRecords.FirstOrDefaultAsync(
            a => a.EmployeeId == ctx.EmployeeId && a.Date == date, ct);

        Guid targetId;
        if (existing is null)
        {
            var record = new AttendanceRecord
            {
                EmployeeId = ctx.EmployeeId,
                Date = date,
                Status = AttendanceStatus.Present,
                LateMinutes = 0,
                ShortageMinutes = 0,
                Source = "AttendanceCorrection",
                ReferenceId = ctx.RequestInstanceId,
                Notes = reason,
            };
            _db.AttendanceRecords.Add(record);
            targetId = record.Id;
        }
        else
        {
            existing.Status = AttendanceStatus.Present;
            existing.LateMinutes = 0;
            existing.ShortageMinutes = 0;
            existing.Source = "AttendanceCorrection";
            existing.Notes = reason;
            targetId = existing.Id;
        }

        return EffectExecutionResult.Ok(
            targetEntityType: "AttendanceRecord",
            targetRecordId: targetId,
            after: new { date, status = nameof(AttendanceStatus.Present) },
            summary: $"Attendance corrected for {date:yyyy-MM-dd}");
    }
}

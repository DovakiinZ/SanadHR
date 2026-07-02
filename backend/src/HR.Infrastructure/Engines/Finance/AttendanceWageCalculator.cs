using HR.Domain.Engines.Finance;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Single owner of the period attendance query, shared by the fact provider (aggregate facts) and
/// the attendance-deduction sync service (per-day drill-down rows) so the two can never drift. Read-only.</summary>
public sealed record AttendanceAggregate(int Days, int OvertimeMinutes, int LateMinutes, int AbsentDays, int ShortageMinutes);

public sealed record AttendanceBreakdownRow(
    Guid EmployeeId, Guid AttendanceRecordId, DateTime Date, AttendancePayrollKind PenaltyKind, int Minutes, int Days);

public sealed class AttendanceWageCalculator
{
    private readonly ApplicationDbContext _db;
    public AttendanceWageCalculator(ApplicationDbContext db) => _db = db;

    /// <summary>Per-employee period aggregate. Shortage on Absent days is excluded (the whole-day absence is
    /// already priced at the daily wage) — identical semantics to the prior inline fact-provider query.</summary>
    public async Task<IReadOnlyDictionary<Guid, AttendanceAggregate>> AggregateAsync(
        IReadOnlyCollection<Guid> employeeIds, PayrollPeriod period, CancellationToken ct)
    {
        if (employeeIds.Count == 0) return new Dictionary<Guid, AttendanceAggregate>();
        return await _db.AttendanceRecords.AsNoTracking()
            .Where(a => employeeIds.Contains(a.EmployeeId) && a.Date >= period.Start && a.Date <= period.End)
            .GroupBy(a => a.EmployeeId)
            .Select(g => new
            {
                EmployeeId = g.Key,
                Agg = new AttendanceAggregate(
                    g.Count(),
                    g.Sum(x => x.OvertimeMinutes),
                    g.Sum(x => x.LateMinutes),
                    g.Count(x => x.Status == AttendanceStatus.Absent),
                    g.Sum(x => x.Status == AttendanceStatus.Absent ? 0 : x.ShortageMinutes)),
            })
            .ToDictionaryAsync(x => x.EmployeeId, x => x.Agg, ct);
    }

    /// <summary>One drill-down row per penalty day: an Absence row for each Absent day, a Late row for each day
    /// with late minutes, a Shortage row for each non-absent day with shortage minutes.</summary>
    public async Task<IReadOnlyList<AttendanceBreakdownRow>> BreakdownRowsAsync(
        IReadOnlyCollection<Guid> employeeIds, PayrollPeriod period, CancellationToken ct)
    {
        if (employeeIds.Count == 0) return Array.Empty<AttendanceBreakdownRow>();
        var days = await _db.AttendanceRecords.AsNoTracking()
            .Where(a => employeeIds.Contains(a.EmployeeId) && a.Date >= period.Start && a.Date <= period.End)
            .Select(a => new { a.Id, a.EmployeeId, a.Date, a.Status, a.LateMinutes, a.ShortageMinutes, a.OvertimeMinutes })
            .ToListAsync(ct);

        var rows = new List<AttendanceBreakdownRow>();
        foreach (var d in days)
        {
            if (d.Status == AttendanceStatus.Absent)
            {
                rows.Add(new AttendanceBreakdownRow(d.EmployeeId, d.Id, d.Date, AttendancePayrollKind.Absence, 0, 1));
                continue; // shortage on an absent day is not double-counted
            }
            if (d.LateMinutes > 0)
                rows.Add(new AttendanceBreakdownRow(d.EmployeeId, d.Id, d.Date, AttendancePayrollKind.Late, d.LateMinutes, 0));
            if (d.ShortageMinutes > 0)
                rows.Add(new AttendanceBreakdownRow(d.EmployeeId, d.Id, d.Date, AttendancePayrollKind.Shortage, d.ShortageMinutes, 0));
            if (d.OvertimeMinutes > 0)
                rows.Add(new AttendanceBreakdownRow(d.EmployeeId, d.Id, d.Date, AttendancePayrollKind.Overtime, d.OvertimeMinutes, 0));
        }
        return rows;
    }
}

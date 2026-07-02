using FluentAssertions;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Attendance;
using HR.Domain.Engines.Finance;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class AttendanceWageCalculatorTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = Array.Empty<string>();
        public bool IsAuthenticated => true;
    }
    private static ApplicationDbContext Ctx(string n) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(n).Options, new FakeUser());
    private static DateTime Utc(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Aggregate_excludes_shortage_on_absent_days_and_counts_absences()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = Guid.NewGuid();
        db.AttendanceRecords.AddRange(
            new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,1), Status = AttendanceStatus.Present, LateMinutes = 30, ShortageMinutes = 0 },
            new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,2), Status = AttendanceStatus.Absent, LateMinutes = 0, ShortageMinutes = 480 });
        await db.SaveChangesAsync();

        var calc = new AttendanceWageCalculator(db);
        var agg = await calc.AggregateAsync(new[] { emp }, new PayrollPeriod(Utc(2026,7,1), Utc(2026,7,31)), default);

        agg[emp].AbsentDays.Should().Be(1);
        agg[emp].LateMinutes.Should().Be(30);
        agg[emp].ShortageMinutes.Should().Be(0); // shortage on the absent day is excluded
    }

    [Fact]
    public async Task BreakdownRows_emits_one_row_per_penalty_day()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = Guid.NewGuid();
        db.AttendanceRecords.AddRange(
            new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,1), Status = AttendanceStatus.Present, LateMinutes = 30 },
            new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,2), Status = AttendanceStatus.Absent },
            new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,3), Status = AttendanceStatus.Present, ShortageMinutes = 60 });
        await db.SaveChangesAsync();

        var calc = new AttendanceWageCalculator(db);
        var rows = await calc.BreakdownRowsAsync(new[] { emp }, new PayrollPeriod(Utc(2026,7,1), Utc(2026,7,31)), default);

        rows.Should().Contain(r => r.PenaltyKind == AttendancePayrollKind.Late && r.Minutes == 30);
        rows.Should().Contain(r => r.PenaltyKind == AttendancePayrollKind.Absence && r.Days == 1);
        rows.Should().Contain(r => r.PenaltyKind == AttendancePayrollKind.Shortage && r.Minutes == 60);
    }
}

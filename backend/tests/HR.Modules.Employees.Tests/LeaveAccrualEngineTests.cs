using FluentAssertions;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Leave;
using HR.Domain.Engines.MasterData;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Leave;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Modules.Employees.Tests;

/// <summary>Verifies ledger-based leave accrual: the 21→30 day rate jump at five effective years,
/// the unpaid-leave seniority shift, and recalculation idempotency.</summary>
public class LeaveAccrualEngineTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public static readonly Guid Tenant = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Tenant;
        public string? Email => "tester@hrcloud.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Leaves.Edit" };
        public bool IsAuthenticated => true;
    }

    private static readonly Guid AnnualTypeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid UnpaidTypeId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private static ApplicationDbContext NewContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApplicationDbContext(options, new FakeUser());
    }

    private static async Task<Guid> SeedEmployeeAsync(string db, DateTime hireDate, params (DateTime start, DateTime end)[] unpaidLeaves)
    {
        using var ctx = NewContext(db);
        var emp = new Employee
        {
            EmployeeNumber = "E-1", FirstName = "Test", LastName = "User", Email = "e1@hrcloud.local",
            HireDate = hireDate, BasicSalary = 10_000m,
        };
        ctx.Employees.Add(emp);

        ctx.MasterDataItems.Add(new MasterDataItem
        {
            Id = AnnualTypeId, ObjectType = MasterDataObjectType.LeaveType, Code = "ANNUAL",
            NameAr = "سنوية", NameEn = "Annual", MetadataJson = "{\"paid\":true}",
        });
        ctx.MasterDataItems.Add(new MasterDataItem
        {
            Id = UnpaidTypeId, ObjectType = MasterDataObjectType.LeaveType, Code = "UNPAID",
            NameAr = "بدون راتب", NameEn = "Unpaid", MetadataJson = "{\"paid\":false}",
        });

        var n = 0;
        foreach (var (start, end) in unpaidLeaves)
            ctx.LeaveRecords.Add(new LeaveRecord
            {
                RecordNumber = $"LV-U-{++n}", EmployeeId = emp.Id, LeaveTypeId = UnpaidTypeId,
                StartDate = DateTime.SpecifyKind(start, DateTimeKind.Utc), EndDate = DateTime.SpecifyKind(end, DateTimeKind.Utc),
                DaysCount = (decimal)((end - start).Days + 1), AffectsBalance = false,
                Status = LeaveRecordStatus.Approved, Source = LeaveRecordSource.HRAssignment,
            });

        await ctx.SaveChangesAsync();
        return emp.Id;
    }

    [Fact]
    public async Task Accrues_21DaysPerYear_BelowFiveYears()
    {
        var db = $"accrual-{Guid.NewGuid()}";
        var empId = await SeedEmployeeAsync(db, DateTime.UtcNow.Date.AddYears(-3));

        using var ctx = NewContext(db);
        var engine = new LeaveAccrualEngine(ctx);
        var posted = await engine.RecalculateAsync(empId, AnnualTypeId);
        var ledger = await engine.GetLedgerAsync(empId, AnnualTypeId);

        posted.Should().BeGreaterThan(30); // ~36 monthly entries over 3 years
        ledger.AccruedToDate.Should().BeApproximately(21m * 3m, 1.5m);
    }

    [Fact]
    public async Task RateJumps_To30DaysPerYear_AfterFiveYears()
    {
        var db = $"accrual-{Guid.NewGuid()}";
        var empId = await SeedEmployeeAsync(db, DateTime.UtcNow.Date.AddYears(-6));

        using var ctx = NewContext(db);
        var engine = new LeaveAccrualEngine(ctx);
        await engine.RecalculateAsync(empId, AnnualTypeId);
        var ledger = await engine.GetLedgerAsync(empId, AnnualTypeId);

        // 5 years @ 21 + 1 year @ 30 = 135 days.
        ledger.AccruedToDate.Should().BeApproximately(21m * 5m + 30m * 1m, 4m);
    }

    [Fact]
    public async Task UnpaidLeave_ShiftsSeniority_AndReducesAccrual()
    {
        var hire = DateTime.UtcNow.Date.AddYears(-3);
        var control = $"accrual-{Guid.NewGuid()}";
        var withUnpaid = $"accrual-{Guid.NewGuid()}";

        var controlId = await SeedEmployeeAsync(control, hire);
        // 60 consecutive unpaid days roughly one year into service.
        var lwpStart = hire.AddDays(365);
        var withId = await SeedEmployeeAsync(withUnpaid, hire, (lwpStart, lwpStart.AddDays(59)));

        decimal controlAccrued, unpaidAccrued;
        using (var c = NewContext(control)) { var e = new LeaveAccrualEngine(c); await e.RecalculateAsync(controlId, AnnualTypeId); controlAccrued = (await e.GetLedgerAsync(controlId, AnnualTypeId)).AccruedToDate; }
        using (var c = NewContext(withUnpaid)) { var e = new LeaveAccrualEngine(c); await e.RecalculateAsync(withId, AnnualTypeId); unpaidAccrued = (await e.GetLedgerAsync(withId, AnnualTypeId)).AccruedToDate; }

        unpaidAccrued.Should().BeLessThan(controlAccrued);
        // ~60 unpaid days at the <5yr rate ≈ 3.45 fewer days accrued.
        (controlAccrued - unpaidAccrued).Should().BeApproximately(60m * (21m / 365.25m), 0.5m);
    }

    [Fact]
    public async Task Recalculate_IsIdempotent_NoDuplicateAccrualRows()
    {
        var db = $"accrual-{Guid.NewGuid()}";
        var empId = await SeedEmployeeAsync(db, DateTime.UtcNow.Date.AddYears(-2));

        using var ctx = NewContext(db);
        var engine = new LeaveAccrualEngine(ctx);

        var firstCount = await engine.RecalculateAsync(empId, AnnualTypeId);
        var firstAccrued = (await engine.GetLedgerAsync(empId, AnnualTypeId)).AccruedToDate;

        var secondCount = await engine.RecalculateAsync(empId, AnnualTypeId);
        var ledger = await engine.GetLedgerAsync(empId, AnnualTypeId);

        secondCount.Should().Be(firstCount);
        ledger.AccruedToDate.Should().Be(firstAccrued);
        ledger.Entries.Count(e => e.Type == nameof(LeaveTransactionType.Accrual)).Should().Be(firstCount);
    }
}

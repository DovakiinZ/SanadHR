# Payroll Sub-project 2D — Attendance → Deduction Records Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn attendance penalties (absence/late/shortage) into visible, traceable `Approved` `PayrollTransaction` deduction records — one per employee/period/penalty-type — that flow through 2C's consume→post→reverse spine, and retire the fact-based `ATTENDANCE_DED` rule so nothing is double-counted.

**Architecture:** A new `AttendanceDeductionSyncService` materializes records idempotently. It reads per-employee wage/attendance numbers from the existing `IPayrollFactProvider` (zero drift vs. the retired rule) and per-day drill-down rows from a thin extracted `AttendanceWageCalculator`. It runs at two call sites (the A+C hybrid): guaranteed inside `PayrollRunEngine.CalculateAsync` before computation, and on-demand via a `POST /api/payroll/attendance-deductions/sync` endpoint. A dedicated `PayrollTransactionAttendanceReference` table (one EF migration) holds the drill-down snapshot. The `ATTENDANCE_DED` rule is removed from the seeder and deactivated on existing tenants.

**Tech Stack:** .NET 8, EF Core 8 + Npgsql (PostgreSQL), xUnit + FluentAssertions + EF InMemory (tests), Next.js (App Router) frontend.

## Global Constraints

- **Target framework:** `net8.0` for all projects (prod + tests).
- **Persistence:** Npgsql/PostgreSQL. All `DateTime` written to the DB **must be `DateTimeKind.Utc`** (`timestamptz` rejects non-UTC). Use an `AsUtc` helper before persisting any date.
- **Tenant + audit are implicit:** inject the concrete `ApplicationDbContext`; it applies the global tenant query filter and stamps `TenantId`/`CreatedBy`/`UpdatedBy`/timestamps in `SaveChangesAsync`. Do **not** inject `ICurrentUserService` unless you need `UserId`/`TenantId` for a domain column the context can't stamp.
- **DI is explicit** (no assembly scan for engines): every service is registered by hand in `HR.Infrastructure/DependencyInjection.cs`. Interfaces live in `HR.Application.Engines.Finance`; implementations in `HR.Infrastructure.Engines.Finance`. Concrete-only engines are registered as themselves (e.g. `AddScoped<PayrollComputation>()`).
- **EF entity config:** via `IEntityTypeConfiguration<T>` classes auto-discovered by `ApplicationDbContext` assembly scan. Tables use `engine_*` snake_case names. Entities derive from `TenantEntity` (gets tenant + soft-delete filter free).
- **Business-rule errors:** throw `DomainException` (maps to HTTP 422, from the 2C hotfix). Illegal state transitions surface via the state-machine exceptions (409).
- **Attendance-sourced records are born `Approved`.** No approval gate. Corrections after posting go through 2C reversal.
- **Migrations:** generated from `backend/src/HR.Infrastructure` with `dotnet ef migrations add <Name> --startup-project ../HR.Api`. Migrations assembly = `HR.Infrastructure`; startup project = `HR.Api`.
- **TDD, frequent commits.** Tests in `backend/tests/HR.Domain.Finance.Tests` (EF InMemory, unique db name per test, `FakeUser : ICurrentUserService`, manual service construction).

---

## File Structure

**Domain (`HR.Domain`)**
- Create `src/HR.Domain/Enums/AttendancePenaltyKind.cs` — the `Absence/Late/Shortage` enum.
- Create `src/HR.Domain/Engines/Finance/Entities/PayrollTransactionAttendanceReference.cs` — drill-down snapshot entity.

**Application (`HR.Application`)**
- Create `src/HR.Application/Engines/Finance/IAttendanceDeductionSyncService.cs` — interface + `AttendanceDeductionSyncReport` + `AttendancePenaltyRow` records.

**Infrastructure (`HR.Infrastructure`)**
- Create `src/HR.Infrastructure/Engines/Finance/AttendanceWageCalculator.cs` — shared attendance aggregate + per-day breakdown rows.
- Create `src/HR.Infrastructure/Engines/Finance/AttendanceDeductionSyncService.cs` — the materialization engine.
- Create `src/HR.Infrastructure/Engines/Finance/PayrollCalcSettings.cs` — tiny `CalcSettingsJson` reader.
- Modify `src/HR.Infrastructure/Engines/Finance/PayrollFactProvider.cs` — use `AttendanceWageCalculator` for the aggregate query.
- Modify `src/HR.Infrastructure/Engines/Finance/PayrollRunEngine.cs` — call sync in `CalculateAsync` before `ComputeAsync`.
- Modify `src/HR.Infrastructure/Engines/Finance/StandardPayrollSeeder.cs` — remove `ATTENDANCE_DED`, add neutralize step.
- Modify `src/HR.Infrastructure/Persistence/ApplicationDbContext.cs` — add DbSet.
- Modify `src/HR.Infrastructure/Persistence/Configurations/Engines/FinanceConfigurations.cs` — add reference-table config.
- Modify `src/HR.Infrastructure/Persistence/MasterDataDefaults.cs` — seed `LATE` + `SHORTAGE`.
- Modify `src/HR.Infrastructure/DependencyInjection.cs` — register the two new services.
- Create migration `src/HR.Infrastructure/Migrations/<ts>_AttendanceDeductionReference.cs` (generated).

**Modules (`HR.Modules`)**
- Modify `src/HR.Modules/Payroll/Controllers/PayrollController.cs` — sync endpoint + breakdown endpoint.
- Modify `src/HR.Modules/Payroll/DTOs/PayrollTransactionDtos.cs` — sync request/report DTOs + breakdown DTO.

**Tests (`HR.Domain.Finance.Tests`)**
- Create `AttendanceWageCalculatorTests.cs`, `AttendanceDeductionSyncServiceTests.cs`, `StandardPayrollSeederAttendanceTests.cs`, and extend run-engine coverage in `AttendanceDeductionRunTests.cs`.

**Frontend**
- Modify the payroll deductions page under `src/app/(dashboard)/payroll/deductions/` (verify exact path at implementation time) — sync button, report toast, breakdown drawer, read-only attendance rows.

---

## Task 1: Enum + reference entity + EF config + migration

**Files:**
- Create: `backend/src/HR.Domain/Enums/AttendancePenaltyKind.cs`
- Create: `backend/src/HR.Domain/Engines/Finance/Entities/PayrollTransactionAttendanceReference.cs`
- Modify: `backend/src/HR.Infrastructure/Persistence/ApplicationDbContext.cs:260` (after the `PayrollTransactions` DbSet)
- Modify: `backend/src/HR.Infrastructure/Persistence/Configurations/Engines/FinanceConfigurations.cs` (append a config class)
- Create: `backend/src/HR.Infrastructure/Migrations/<ts>_AttendanceDeductionReference.cs` (generated)

**Interfaces:**
- Produces: `enum AttendancePenaltyKind { Absence = 1, Late = 2, Shortage = 3 }`; entity `PayrollTransactionAttendanceReference : TenantEntity` with `Guid PayrollTransactionId`, `Guid AttendanceRecordId`, `DateTime Date`, `AttendancePenaltyKind PenaltyKind`, `int Minutes`, `int Days`, `decimal AmountContribution`.

- [ ] **Step 1: Write the failing test** — `backend/tests/HR.Domain.Finance.Tests/AttendanceReferenceEntityTests.cs`

```csharp
using FluentAssertions;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using HR.Application.Common.Interfaces;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class AttendanceReferenceEntityTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = Array.Empty<string>();
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    [Fact]
    public async Task Can_persist_and_read_attendance_reference()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var txnId = Guid.NewGuid();
        var recId = Guid.NewGuid();
        db.PayrollTransactionAttendanceReferences.Add(new PayrollTransactionAttendanceReference
        {
            PayrollTransactionId = txnId, AttendanceRecordId = recId,
            Date = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc),
            PenaltyKind = AttendancePenaltyKind.Late, Minutes = 30, Days = 0, AmountContribution = 12.5m,
        });
        await db.SaveChangesAsync();

        var row = await db.PayrollTransactionAttendanceReferences.SingleAsync(r => r.PayrollTransactionId == txnId);
        row.PenaltyKind.Should().Be(AttendancePenaltyKind.Late);
        row.AmountContribution.Should().Be(12.5m);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter AttendanceReferenceEntityTests`
Expected: FAIL — `AttendancePenaltyKind` / `PayrollTransactionAttendanceReference` / `PayrollTransactionAttendanceReferences` do not exist (compile error).

- [ ] **Step 3: Create the enum** — `backend/src/HR.Domain/Enums/AttendancePenaltyKind.cs`

```csharp
namespace HR.Domain.Enums;

/// <summary>Business semantics for an attendance-driven deduction. The engine keys on this enum; the
/// customer-configurable DeductionType master-data item (codes ABSENCE/LATE/SHORTAGE) supplies labels only.</summary>
public enum AttendancePenaltyKind
{
    Absence = 1,
    Late = 2,
    Shortage = 3,
}
```

- [ ] **Step 4: Create the entity** — `backend/src/HR.Domain/Engines/Finance/Entities/PayrollTransactionAttendanceReference.cs`

```csharp
using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Finance.Entities;

/// <summary>One contributing attendance day behind an attendance-sourced PayrollTransaction deduction.
/// Snapshotted at sync time so the breakdown drawer and audit stay accurate even if the underlying
/// attendance record later changes (overview §19 — never overwrite original business information).</summary>
public class PayrollTransactionAttendanceReference : TenantEntity
{
    /// <summary>The attendance deduction record this row explains.</summary>
    public Guid PayrollTransactionId { get; set; }

    /// <summary>The attendance day that produced this share of the deduction.</summary>
    public Guid AttendanceRecordId { get; set; }

    /// <summary>Snapshot of the attendance date.</summary>
    public DateTime Date { get; set; }

    public AttendancePenaltyKind PenaltyKind { get; set; }

    /// <summary>Late/shortage minutes for this day (0 for absence).</summary>
    public int Minutes { get; set; }

    /// <summary>Absent days for this row (1 for an absence day, else 0).</summary>
    public int Days { get; set; }

    /// <summary>This row's share of the deduction amount (pre-rounding; the transaction Amount is authoritative).</summary>
    public decimal AmountContribution { get; set; }
}
```

- [ ] **Step 5: Add the DbSet** — `backend/src/HR.Infrastructure/Persistence/ApplicationDbContext.cs`, immediately after the `PayrollTransactions` DbSet (line 260)

```csharp
    public DbSet<HR.Domain.Engines.Finance.Entities.PayrollTransactionAttendanceReference> PayrollTransactionAttendanceReferences => Set<HR.Domain.Engines.Finance.Entities.PayrollTransactionAttendanceReference>();
```

- [ ] **Step 6: Add the EF config** — append to `backend/src/HR.Infrastructure/Persistence/Configurations/Engines/FinanceConfigurations.cs`

```csharp
public class PayrollTransactionAttendanceReferenceConfiguration : IEntityTypeConfiguration<PayrollTransactionAttendanceReference>
{
    public void Configure(EntityTypeBuilder<PayrollTransactionAttendanceReference> builder)
    {
        builder.ToTable("engine_payroll_transaction_attendance_refs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AmountContribution).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.PayrollTransactionId);
        builder.HasIndex(x => x.AttendanceRecordId);

        builder.HasOne<PayrollTransaction>()
            .WithMany()
            .HasForeignKey(x => x.PayrollTransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

(If `PayrollTransaction` isn't already imported in this file, it is — the file configures it at `:256`. No new `using` needed.)

- [ ] **Step 7: Run test to verify it passes**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter AttendanceReferenceEntityTests`
Expected: PASS.

- [ ] **Step 8: Generate the migration**

Run: `cd backend/src/HR.Infrastructure && dotnet ef migrations add AttendanceDeductionReference --startup-project ../HR.Api`
Expected: creates `Migrations/<ts>_AttendanceDeductionReference.cs` with `CreateTable("engine_payroll_transaction_attendance_refs", ...)`. Open it and confirm it creates only the new table (no unintended drops).

- [ ] **Step 9: Verify build**

Run: `cd backend && dotnet build`
Expected: 0 errors.

- [ ] **Step 10: Commit**

```bash
git add backend/src/HR.Domain/Enums/AttendancePenaltyKind.cs \
        backend/src/HR.Domain/Engines/Finance/Entities/PayrollTransactionAttendanceReference.cs \
        backend/src/HR.Infrastructure/Persistence/ApplicationDbContext.cs \
        backend/src/HR.Infrastructure/Persistence/Configurations/Engines/FinanceConfigurations.cs \
        backend/src/HR.Infrastructure/Migrations \
        backend/tests/HR.Domain.Finance.Tests/AttendanceReferenceEntityTests.cs
git commit -m "feat(payroll-2d): AttendancePenaltyKind + attendance-reference table + migration"
```

---

## Task 2: Extract `AttendanceWageCalculator` and refactor the fact provider

**Files:**
- Create: `backend/src/HR.Infrastructure/Engines/Finance/AttendanceWageCalculator.cs`
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/PayrollFactProvider.cs:85-97` (aggregate query) and constructor
- Modify: `backend/src/HR.Infrastructure/DependencyInjection.cs:71` (register concrete)
- Test: `backend/tests/HR.Domain.Finance.Tests/AttendanceWageCalculatorTests.cs`

**Interfaces:**
- Consumes: `ApplicationDbContext`, `AttendanceRecord`, `AttendanceStatus`.
- Produces:
  - `sealed record AttendanceAggregate(int Days, int OvertimeMinutes, int LateMinutes, int AbsentDays, int ShortageMinutes);`
  - `sealed record AttendanceBreakdownRow(Guid EmployeeId, Guid AttendanceRecordId, DateTime Date, AttendancePenaltyKind PenaltyKind, int Minutes, int Days);`
  - `Task<IReadOnlyDictionary<Guid, AttendanceAggregate>> AggregateAsync(IReadOnlyCollection<Guid> employeeIds, PayrollPeriod period, CancellationToken ct)`
  - `Task<IReadOnlyList<AttendanceBreakdownRow>> BreakdownRowsAsync(IReadOnlyCollection<Guid> employeeIds, PayrollPeriod period, CancellationToken ct)`

- [ ] **Step 1: Write the failing test** — `AttendanceWageCalculatorTests.cs`

```csharp
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

        rows.Should().Contain(r => r.PenaltyKind == AttendancePenaltyKind.Late && r.Minutes == 30);
        rows.Should().Contain(r => r.PenaltyKind == AttendancePenaltyKind.Absence && r.Days == 1);
        rows.Should().Contain(r => r.PenaltyKind == AttendancePenaltyKind.Shortage && r.Minutes == 60);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter AttendanceWageCalculatorTests`
Expected: FAIL — `AttendanceWageCalculator` does not exist.

- [ ] **Step 3: Create `AttendanceWageCalculator`** — `backend/src/HR.Infrastructure/Engines/Finance/AttendanceWageCalculator.cs`

```csharp
using HR.Domain.Engines.Finance;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Single owner of the period attendance query, shared by the fact provider (aggregate facts) and
/// the attendance-deduction sync service (per-day drill-down rows) so the two can never drift. Read-only.</summary>
public sealed record AttendanceAggregate(int Days, int OvertimeMinutes, int LateMinutes, int AbsentDays, int ShortageMinutes);

public sealed record AttendanceBreakdownRow(
    Guid EmployeeId, Guid AttendanceRecordId, DateTime Date, AttendancePenaltyKind PenaltyKind, int Minutes, int Days);

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
            .Select(a => new { a.Id, a.EmployeeId, a.Date, a.Status, a.LateMinutes, a.ShortageMinutes })
            .ToListAsync(ct);

        var rows = new List<AttendanceBreakdownRow>();
        foreach (var d in days)
        {
            if (d.Status == AttendanceStatus.Absent)
            {
                rows.Add(new AttendanceBreakdownRow(d.EmployeeId, d.Id, d.Date, AttendancePenaltyKind.Absence, 0, 1));
                continue; // shortage on an absent day is not double-counted
            }
            if (d.LateMinutes > 0)
                rows.Add(new AttendanceBreakdownRow(d.EmployeeId, d.Id, d.Date, AttendancePenaltyKind.Late, d.LateMinutes, 0));
            if (d.ShortageMinutes > 0)
                rows.Add(new AttendanceBreakdownRow(d.EmployeeId, d.Id, d.Date, AttendancePenaltyKind.Shortage, d.ShortageMinutes, 0));
        }
        return rows;
    }
}
```

- [ ] **Step 4: Run the calculator test to verify it passes**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter AttendanceWageCalculatorTests`
Expected: PASS.

- [ ] **Step 5: Refactor `PayrollFactProvider` to use the calculator's aggregate**

In `backend/src/HR.Infrastructure/Engines/Finance/PayrollFactProvider.cs`, inject the calculator and replace the inline aggregate. Change the constructor (currently `:21`):

```csharp
    private readonly ApplicationDbContext _db;
    private readonly IScopeEngine _scope;
    private readonly AttendanceWageCalculator _attendance;

    public PayrollFactProvider(ApplicationDbContext db, IScopeEngine scope, AttendanceWageCalculator attendance)
    { _db = db; _scope = scope; _attendance = attendance; }
```

Replace the inline attendance query block (`:85-97`) with:

```csharp
        // Attendance aggregates for the period (shared with the attendance-deduction sync so records and
        // the inert facts can never drift). Shortage on Absent days is excluded inside the calculator.
        var attendance = (await _attendance.AggregateAsync(empIds, period, ct))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
```

The downstream reads still work because `AttendanceAggregate` exposes the same members (`Days`, `OvertimeMinutes`, `LateMinutes`, `AbsentDays`, `ShortageMinutes`). Confirm the later usages (`att.Days`, `att.AbsentDays`, `att.LateMinutes`, `att.ShortageMinutes`, `att.OvertimeMinutes` at `:122-131,142-143`) compile unchanged.

- [ ] **Step 6: Register the calculator in DI** — `backend/src/HR.Infrastructure/DependencyInjection.cs`, after the `PayrollComputation` registration (`:71`)

```csharp
        services.AddScoped<HR.Infrastructure.Engines.Finance.AttendanceWageCalculator>();
```

- [ ] **Step 7: Run the full finance test suite to prove no regression**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests`
Expected: PASS (all prior tests + the two new calculator tests). If any fact-provider test relied on the old inline shape, it still passes because the aggregate members are identical.

- [ ] **Step 8: Commit**

```bash
git add backend/src/HR.Infrastructure/Engines/Finance/AttendanceWageCalculator.cs \
        backend/src/HR.Infrastructure/Engines/Finance/PayrollFactProvider.cs \
        backend/src/HR.Infrastructure/DependencyInjection.cs \
        backend/tests/HR.Domain.Finance.Tests/AttendanceWageCalculatorTests.cs
git commit -m "refactor(payroll-2d): extract AttendanceWageCalculator; fact provider reuses it"
```

---

## Task 3: `AttendanceDeductionSyncService` (core materialization)

**Files:**
- Create: `backend/src/HR.Application/Engines/Finance/IAttendanceDeductionSyncService.cs`
- Create: `backend/src/HR.Infrastructure/Engines/Finance/PayrollCalcSettings.cs`
- Create: `backend/src/HR.Infrastructure/Engines/Finance/AttendanceDeductionSyncService.cs`
- Modify: `backend/src/HR.Infrastructure/DependencyInjection.cs` (register the service)
- Test: `backend/tests/HR.Domain.Finance.Tests/AttendanceDeductionSyncServiceTests.cs`

**Interfaces:**
- Consumes: `IPayrollFactProvider.BuildInputsAsync` (facts `DailyWage`,`HourlyWage`,`AbsentDays`,`LateHours`,`ShortageHours`), `AttendanceWageCalculator.BreakdownRowsAsync`, `ApplicationDbContext`, `PayrollDefinitionVersion`, `PayrollPeriod`.
- Produces:
  - `sealed record AttendanceDeductionSyncReport(int Created, int Updated, int Removed, int SkippedPosted, int TotalProcessed);`
  - `interface IAttendanceDeductionSyncService { Task<AttendanceDeductionSyncReport> SyncAsync(PayrollDefinitionVersion version, PayrollPeriod period, IReadOnlyCollection<Guid> employeeIds, CancellationToken ct); }`

**Design notes (read before implementing):**
- Records are stamped `EffectiveDate = period.Start`, `TargetPeriodYear/Month = period.Year/Month` so the 2C consumer places them in **this** run's period (attendance penalties are period-scoped, never cutoff-carried). `TransactionDate = period.End`. Born `Approved`.
- Persistent discriminator per kind = the `DeductionType` master-data **TypeId**, resolved by code (`ABSENCE`/`LATE`/`SHORTAGE`). Upsert key = `(EmployeeId, TargetPeriodYear, TargetPeriodMonth, TypeId, SourceModule == "Attendance")`, ignoring `Reversed`/`Cancelled` rows.
- The transaction `Amount` is computed from the **facts** (`AbsentDays*DailyWage`, `LateHours*HourlyWage`, `ShortageHours*HourlyWage`, each `Round(...,2)`) — identical to the retired rule, so zero drift. Drill-down rows are priced from the same `DailyWage`/`HourlyWage`.
- Missing master-data type → `DomainException` (config error; T5 seeds them).

- [ ] **Step 1: Write the failing tests** — `AttendanceDeductionSyncServiceTests.cs`

```csharp
using FluentAssertions;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Application.Engines.Scope;
using HR.Domain.Engines.Attendance;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.MasterData;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class AttendanceDeductionSyncServiceTests
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

    private static AttendanceDeductionSyncService Svc(ApplicationDbContext db)
    {
        // PayrollFactProvider never dereferences IScopeEngine when an explicit employee population is passed
        // (BuildInputsAsync:41), and the sync service always passes one — so null scope is safe in tests.
        var facts = new PayrollFactProvider(db, null!, new AttendanceWageCalculator(db));
        return new AttendanceDeductionSyncService(db, facts, new AttendanceWageCalculator(db));
    }

    // Seeds an employee (30-day-basis daily wage = 3000/30 = 100/day, hourly = 12.5) + the 3 deduction types.
    private static async Task<Guid> SeedAsync(ApplicationDbContext db)
    {
        var emp = new Employee { EmployeeNumber = "E1", FirstName = "Ali", LastName = "S", Email = "a@t.local", BasicSalary = 3000m };
        db.Employees.Add(emp);
        foreach (var code in new[] { "ABSENCE", "LATE", "SHORTAGE" })
            db.MasterDataItems.Add(new MasterDataItem { ObjectType = MasterDataObjectType.DeductionType, Code = code, NameAr = code, NameEn = code });
        await db.SaveChangesAsync();
        return emp.Id;
    }

    private static PayrollDefinitionVersion Version() => new()
    { DayBasis = DayBasis.Fixed30, CutoffDay = 27, CarryToNextPeriod = true, Currency = "SAR" };

    [Fact]
    public async Task Creates_one_approved_record_per_penalty_kind()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = await SeedAsync(db);
        db.AttendanceRecords.AddRange(
            new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,2), Status = AttendanceStatus.Absent },
            new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,3), Status = AttendanceStatus.Present, LateMinutes = 60 });
        await db.SaveChangesAsync();

        var report = await Svc(db).SyncAsync(Version(), new PayrollPeriod(Utc(2026,7,1), Utc(2026,7,31)), new[] { emp }, default);

        report.Created.Should().Be(2); // absence + late (no shortage)
        var txns = await db.PayrollTransactions.Where(t => t.EmployeeId == emp).ToListAsync();
        txns.Should().OnlyContain(t => t.SourceModule == "Attendance" && t.Status == PayrollTransactionStatus.Approved
            && t.Kind == PayrollTransactionKind.Deduction && t.TargetPeriodYear == 2026 && t.TargetPeriodMonth == 7);
        txns.Sum(t => t.Amount).Should().Be(100m + 12.5m); // 1 absent day * 100 + 1 late hr * 12.5
    }

    [Fact]
    public async Task Resync_updates_in_place_and_is_idempotent()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = await SeedAsync(db);
        db.AttendanceRecords.Add(new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,2), Status = AttendanceStatus.Absent });
        await db.SaveChangesAsync();
        var period = new PayrollPeriod(Utc(2026,7,1), Utc(2026,7,31));

        var first = await Svc(db).SyncAsync(Version(), period, new[] { emp }, default);
        var second = await Svc(db).SyncAsync(Version(), period, new[] { emp }, default);

        first.Created.Should().Be(1);
        second.Created.Should().Be(0);
        second.Updated.Should().Be(1);
        (await db.PayrollTransactions.CountAsync(t => t.EmployeeId == emp && t.Status == PayrollTransactionStatus.Approved))
            .Should().Be(1); // no duplicate
    }

    [Fact]
    public async Task Cleared_penalty_cancels_the_stale_record()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = await SeedAsync(db);
        var rec = new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,2), Status = AttendanceStatus.Absent };
        db.AttendanceRecords.Add(rec);
        await db.SaveChangesAsync();
        var period = new PayrollPeriod(Utc(2026,7,1), Utc(2026,7,31));
        await Svc(db).SyncAsync(Version(), period, new[] { emp }, default);

        rec.Status = AttendanceStatus.Present; // correction cleared the absence
        await db.SaveChangesAsync();
        var report = await Svc(db).SyncAsync(Version(), period, new[] { emp }, default);

        report.Removed.Should().Be(1);
        (await db.PayrollTransactions.SingleAsync(t => t.EmployeeId == emp)).Status
            .Should().Be(PayrollTransactionStatus.Cancelled);
    }

    [Fact]
    public async Task Posted_record_is_skipped_not_touched()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = await SeedAsync(db);
        db.AttendanceRecords.Add(new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,2), Status = AttendanceStatus.Absent });
        await db.SaveChangesAsync();
        var period = new PayrollPeriod(Utc(2026,7,1), Utc(2026,7,31));
        await Svc(db).SyncAsync(Version(), period, new[] { emp }, default);

        var txn = await db.PayrollTransactions.SingleAsync(t => t.EmployeeId == emp);
        txn.Status = PayrollTransactionStatus.Posted; // simulate a completed run
        await db.SaveChangesAsync();
        var report = await Svc(db).SyncAsync(Version(), period, new[] { emp }, default);

        report.SkippedPosted.Should().Be(1);
        report.Updated.Should().Be(0);
    }

    [Fact]
    public async Task Toggle_off_is_a_noop()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = await SeedAsync(db);
        db.AttendanceRecords.Add(new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,2), Status = AttendanceStatus.Absent });
        await db.SaveChangesAsync();
        var v = Version();
        v.CalcSettingsJson = "{\"includeAttendanceDeductions\":false}";

        var report = await Svc(db).SyncAsync(v, new PayrollPeriod(Utc(2026,7,1), Utc(2026,7,31)), new[] { emp }, default);

        report.TotalProcessed.Should().Be(0);
        (await db.PayrollTransactions.AnyAsync(t => t.EmployeeId == emp)).Should().BeFalse();
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter AttendanceDeductionSyncServiceTests`
Expected: FAIL — `AttendanceDeductionSyncService` does not exist.

- [ ] **Step 3: Create the `CalcSettingsJson` reader** — `backend/src/HR.Infrastructure/Engines/Finance/PayrollCalcSettings.cs`

```csharp
using System.Text.Json;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Minimal reader for the version's CalcSettingsJson toggles. Absent/invalid → feature enabled.</summary>
public static class PayrollCalcSettings
{
    public static bool IncludeAttendanceDeductions(string? calcSettingsJson)
    {
        if (string.IsNullOrWhiteSpace(calcSettingsJson)) return true;
        try
        {
            using var doc = JsonDocument.Parse(calcSettingsJson);
            if (doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("includeAttendanceDeductions", out var v)
                && v.ValueKind == JsonValueKind.False)
                return false;
        }
        catch (JsonException) { /* malformed → default enabled */ }
        return true;
    }
}
```

- [ ] **Step 4: Create the interface + report** — `backend/src/HR.Application/Engines/Finance/IAttendanceDeductionSyncService.cs`

```csharp
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;

namespace HR.Application.Engines.Finance;

/// <summary>Outcome of an attendance-deduction sync pass.</summary>
public sealed record AttendanceDeductionSyncReport(int Created, int Updated, int Removed, int SkippedPosted, int TotalProcessed);

/// <summary>Materializes attendance penalties (absence/late/shortage) into Approved PayrollTransaction
/// deduction records — one per employee/period/kind — idempotently. The single source of truth for
/// attendance deductions (the ATTENDANCE_DED rule is retired).</summary>
public interface IAttendanceDeductionSyncService
{
    Task<AttendanceDeductionSyncReport> SyncAsync(
        PayrollDefinitionVersion version, PayrollPeriod period,
        IReadOnlyCollection<Guid> employeeIds, CancellationToken ct = default);
}
```

- [ ] **Step 5: Create the service** — `backend/src/HR.Infrastructure/Engines/Finance/AttendanceDeductionSyncService.cs`

```csharp
using HR.Application.Common.Exceptions;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.MasterData;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

public sealed class AttendanceDeductionSyncService : IAttendanceDeductionSyncService
{
    private const string Source = "Attendance";
    private const string RefType = "AttendancePeriodPenalty";

    private static readonly (AttendancePenaltyKind Kind, string Code)[] KindCodes =
    {
        (AttendancePenaltyKind.Absence, "ABSENCE"),
        (AttendancePenaltyKind.Late, "LATE"),
        (AttendancePenaltyKind.Shortage, "SHORTAGE"),
    };

    private readonly ApplicationDbContext _db;
    private readonly IPayrollFactProvider _facts;
    private readonly AttendanceWageCalculator _attendance;

    public AttendanceDeductionSyncService(ApplicationDbContext db, IPayrollFactProvider facts, AttendanceWageCalculator attendance)
    { _db = db; _facts = facts; _attendance = attendance; }

    public async Task<AttendanceDeductionSyncReport> SyncAsync(
        PayrollDefinitionVersion version, PayrollPeriod period,
        IReadOnlyCollection<Guid> employeeIds, CancellationToken ct = default)
    {
        if (employeeIds.Count == 0 || !PayrollCalcSettings.IncludeAttendanceDeductions(version.CalcSettingsJson))
            return new AttendanceDeductionSyncReport(0, 0, 0, 0, 0);

        // Resolve the DeductionType master-data ids by code (config errors surface clearly).
        var typeByKind = await ResolveTypesAsync(ct);

        // Wages + aggregate penalty inputs, straight from the fact provider (identical to the retired rule).
        var inputs = await _facts.BuildInputsAsync(version, period, employeeIds, ct);
        var factsByEmp = inputs.ToDictionary(i => i.EmployeeId, i => i.Facts);

        // Per-day drill-down rows for the reference snapshot.
        var rowsByEmp = (await _attendance.BreakdownRowsAsync(employeeIds, period, ct))
            .GroupBy(r => r.EmployeeId).ToDictionary(g => g.Key, g => g.ToList());

        // Existing attendance-sourced records for this period (any live status).
        var existing = await _db.PayrollTransactions
            .Where(t => t.SourceModule == Source && employeeIds.Contains(t.EmployeeId)
                        && t.TargetPeriodYear == period.Year && t.TargetPeriodMonth == period.Month)
            .ToListAsync(ct);
        var existingByKey = existing
            .GroupBy(t => (t.EmployeeId, t.TypeId))
            .ToDictionary(g => g.Key, g => g.First());

        int created = 0, updated = 0, removed = 0, skipped = 0, processed = 0;

        foreach (var empId in employeeIds)
        {
            if (!factsByEmp.TryGetValue(empId, out var f)) continue;
            var dailyWage = Dec(f, "DailyWage");
            var hourlyWage = Dec(f, "HourlyWage");

            foreach (var (kind, code) in KindCodes)
            {
                processed++;
                var amount = kind switch
                {
                    AttendancePenaltyKind.Absence => Math.Round(Dec(f, "AbsentDays") * dailyWage, 2),
                    AttendancePenaltyKind.Late => Math.Round(Dec(f, "LateHours") * hourlyWage, 2),
                    AttendancePenaltyKind.Shortage => Math.Round(Dec(f, "ShortageHours") * hourlyWage, 2),
                    _ => 0m,
                };
                var typeId = typeByKind[kind];
                existingByKey.TryGetValue((empId, typeId), out var txn);

                if (txn is { Status: PayrollTransactionStatus.Posted })
                { skipped++; continue; }

                if (amount <= 0m)
                {
                    if (txn is not null && txn.Status != PayrollTransactionStatus.Cancelled)
                    {
                        txn.Status = PayrollTransactionStatus.Cancelled;
                        txn.StatusReason = "Attendance penalty cleared on re-sync.";
                        await ClearRefsAsync(txn.Id, ct);
                        removed++;
                    }
                    continue;
                }

                if (txn is null)
                {
                    txn = new PayrollTransaction
                    {
                        Kind = PayrollTransactionKind.Deduction,
                        EmployeeId = empId,
                        TypeId = typeId,
                        Amount = amount,
                        EffectiveDate = period.Start,           // period-scoped: never cutoff-carried
                        TransactionDate = period.End,
                        TargetPeriodYear = period.Year,
                        TargetPeriodMonth = period.Month,
                        SourceModule = Source,
                        ReferenceType = RefType,
                        Status = PayrollTransactionStatus.Approved,
                    };
                    _db.PayrollTransactions.Add(txn);
                    await _db.SaveChangesAsync(ct);        // materialize Id for the reference rows
                    created++;
                }
                else
                {
                    txn.Amount = amount;
                    if (txn.Status == PayrollTransactionStatus.Cancelled)
                        txn.Status = PayrollTransactionStatus.Approved; // penalty reappeared
                    updated++;
                }

                await WriteRefsAsync(txn.Id, kind, dailyWage, hourlyWage,
                    rowsByEmp.TryGetValue(empId, out var rs) ? rs : new(), ct);
            }
        }

        await _db.SaveChangesAsync(ct);
        return new AttendanceDeductionSyncReport(created, updated, removed, skipped, processed);
    }

    private async Task<IReadOnlyDictionary<AttendancePenaltyKind, Guid>> ResolveTypesAsync(CancellationToken ct)
    {
        var codes = KindCodes.Select(k => k.Code).ToArray();
        var found = await _db.MasterDataItems.AsNoTracking()
            .Where(m => m.ObjectType == MasterDataObjectType.DeductionType && codes.Contains(m.Code))
            .Select(m => new { m.Code, m.Id }).ToListAsync(ct);
        var byCode = found.ToDictionary(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<AttendancePenaltyKind, Guid>();
        foreach (var (kind, code) in KindCodes)
        {
            if (!byCode.TryGetValue(code, out var id))
                throw new DomainException($"No DeductionType configured for attendance penalty '{kind}' (code {code}). Re-run payroll bootstrap.");
            map[kind] = id;
        }
        return map;
    }

    private async Task WriteRefsAsync(Guid txnId, AttendancePenaltyKind kind, decimal dailyWage, decimal hourlyWage,
        List<AttendanceBreakdownRow> rows, CancellationToken ct)
    {
        await ClearRefsAsync(txnId, ct);
        foreach (var r in rows.Where(r => r.PenaltyKind == kind))
        {
            var contribution = kind == AttendancePenaltyKind.Absence
                ? dailyWage * r.Days
                : Math.Round(r.Minutes / 60m, 2) * hourlyWage;
            _db.PayrollTransactionAttendanceReferences.Add(new PayrollTransactionAttendanceReference
            {
                PayrollTransactionId = txnId, AttendanceRecordId = r.AttendanceRecordId,
                Date = r.Date, PenaltyKind = kind, Minutes = r.Minutes, Days = r.Days,
                AmountContribution = contribution,
            });
        }
    }

    private async Task ClearRefsAsync(Guid txnId, CancellationToken ct)
    {
        var old = await _db.PayrollTransactionAttendanceReferences
            .Where(r => r.PayrollTransactionId == txnId).ToListAsync(ct);
        if (old.Count > 0) _db.PayrollTransactionAttendanceReferences.RemoveRange(old);
    }

    private static decimal Dec(IReadOnlyDictionary<string, object?> facts, string key) =>
        facts.TryGetValue(key, out var v) && v is not null ? Convert.ToDecimal(v) : 0m;
}
```

- [ ] **Step 6: Register the service in DI** — `backend/src/HR.Infrastructure/DependencyInjection.cs`, after the `AttendanceWageCalculator` line from Task 2

```csharp
        services.AddScoped<HR.Application.Engines.Finance.IAttendanceDeductionSyncService, HR.Infrastructure.Engines.Finance.AttendanceDeductionSyncService>();
```

- [ ] **Step 7: Run the sync tests to verify they pass**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter AttendanceDeductionSyncServiceTests`
Expected: PASS (5 tests). If `DomainException`'s namespace differs, fix the `using` (search: `grep -rn "class DomainException" backend/src`).

- [ ] **Step 8: Commit**

```bash
git add backend/src/HR.Application/Engines/Finance/IAttendanceDeductionSyncService.cs \
        backend/src/HR.Infrastructure/Engines/Finance/PayrollCalcSettings.cs \
        backend/src/HR.Infrastructure/Engines/Finance/AttendanceDeductionSyncService.cs \
        backend/src/HR.Infrastructure/DependencyInjection.cs \
        backend/tests/HR.Domain.Finance.Tests/AttendanceDeductionSyncServiceTests.cs
git commit -m "feat(payroll-2d): AttendanceDeductionSyncService (idempotent per-kind upsert + refs + report)"
```

---

## Task 4: Wire sync into `PayrollRunEngine.CalculateAsync` (the guarantee)

**Files:**
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/PayrollRunEngine.cs` (constructor + `CalculateAsync:103-117`)
- Test: `backend/tests/HR.Domain.Finance.Tests/AttendanceDeductionRunTests.cs`

**Interfaces:**
- Consumes: `IAttendanceDeductionSyncService.SyncAsync`.
- Produces: attendance records materialized + consumed within a single `CalculateAsync`.

This is a **genuine `CalculateAsync` integration test** — it drives the real run engine end-to-end and asserts attendance records are materialized *by the engine* (not by a direct sync call). `CalculateAsync` reads its population from the pre-seeded `PayrollRunPopulation` rows and never calls `IScopeEngine` or `IPayrollValidationEngine`, so those two deps are safe to stub. It asserts **materialization** (an Approved attendance record exists after Calculate); Task 5 separately proves the retired rule no longer double-counts, so this test does not assert exact net.

Engine ctors (verified): `RuleEngine(ApplicationDbContext)`, `PayrollValidationEngine(IEnumerable<IPayrollValidator>)`, `PayrollComputation(db, IPayrollFactProvider, IRuleEngine, IPayrollTransactionConsumer)`, `PayrollRunEngine(db, PayrollComputation, IPayrollValidationEngine, ICurrentUserService, IAuditLogService, IScopeEngine, IAttendanceDeductionSyncService)`.

- [ ] **Step 1: Write the failing test** — `AttendanceDeductionRunTests.cs`

```csharp
using FluentAssertions;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Attendance;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.MasterData;
using HR.Modules.Employees.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

// NOTE: Employee lives in HR.Modules.Employees.Entities. IPayrollValidator / PayrollRun / PayrollRunPopulation
// namespaces: resolve by searching if the compiler complains (they are HR.Application.Engines.Finance and
// HR.Domain.Engines.Finance.Entities respectively).
namespace HR.Domain.Finance.Tests;

public class AttendanceDeductionRunTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = Array.Empty<string>();
        public bool IsAuthenticated => true;
    }
    // Audit is invoked by CalculateAsync; a no-op fake keeps the test focused.
    private sealed class FakeAudit : IAuditLogService
    {
        public Task LogAsync(string action, string entityType, Guid entityId, object? before, object? after, CancellationToken ct = default)
            => Task.CompletedTask;
    }
    private static ApplicationDbContext Ctx(string n) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(n).Options, new FakeUser());
    private static DateTime Utc(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    private static PayrollRunEngine Engine(ApplicationDbContext db)
    {
        var calc = new AttendanceWageCalculator(db);
        var facts = new PayrollFactProvider(db, null!, calc); // scope unused: population is explicit
        var computation = new PayrollComputation(db, facts, new RuleEngine(db), new PayrollTransactionConsumer(db));
        var sync = new AttendanceDeductionSyncService(db, facts, calc);
        return new PayrollRunEngine(db, computation,
            new PayrollValidationEngine(Array.Empty<IPayrollValidator>()),
            new FakeUser(), new FakeAudit(), null!, sync); // scope null!: CalculateAsync never calls it
    }

    [Fact]
    public async Task Calculate_materializes_approved_attendance_records_even_without_sync_now()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var defId = await new StandardPayrollSeeder(db).EnsureStandardMonthlyAsync();
        var version = await db.PayrollDefinitionVersions.FirstAsync(v => v.PayrollDefinitionId == defId);

        var emp = new Employee { EmployeeNumber = "E1", FirstName = "Ali", LastName = "S", Email = "a@t.local", BasicSalary = 3000m };
        db.Employees.Add(emp);
        foreach (var code in new[] { "ABSENCE", "LATE", "SHORTAGE" })
            db.MasterDataItems.Add(new MasterDataItem { ObjectType = MasterDataObjectType.DeductionType, Code = code, NameAr = code, NameEn = code });
        db.AttendanceRecords.Add(new AttendanceRecord { EmployeeId = emp.Id, Date = Utc(2026,7,2), Status = AttendanceStatus.Absent });
        await db.SaveChangesAsync();

        var run = new PayrollRun
        {
            RunNumber = "PR-TEST-1", PayrollDefinitionId = defId, PayrollDefinitionVersionId = version.Id,
            RuleSetVersionId = version.RuleSetVersionId, PeriodStart = Utc(2026,7,1), PeriodEnd = Utc(2026,7,31),
            State = PayrollRunState.Draft, Currency = "SAR",
        };
        db.PayrollRuns.Add(run);
        db.PayrollRunPopulations.Add(new PayrollRunPopulation { PayrollRunId = run.Id, EmployeeId = emp.Id, IsIncluded = true });
        await db.SaveChangesAsync();

        await Engine(db).CalculateAsync(run.Id);

        (await db.PayrollTransactions.CountAsync(t => t.SourceModule == "Attendance"
            && t.Status == PayrollTransactionStatus.Approved && t.EmployeeId == emp.Id)).Should().Be(1);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter AttendanceDeductionRunTests`
Expected: FAIL — the test won't compile until `PayrollRunEngine`'s constructor takes `IAttendanceDeductionSyncService` (Step 3). After the ctor change but before Step 4's wiring, it compiles and FAILS the assertion (0 attendance records — Calculate doesn't yet materialize them). That is the genuine RED for this task.

If `IAuditLogService.LogAsync`'s signature differs from the fake above, match it exactly (check `src/HR.Application/Common/Interfaces/IAuditLogService.cs`).

- [ ] **Step 3: Inject the sync service into `PayrollRunEngine`**

In `backend/src/HR.Infrastructure/Engines/Finance/PayrollRunEngine.cs`, add the field + constructor param (constructor at `:29-43`):

```csharp
    private readonly IAttendanceDeductionSyncService _attendanceSync;
```

Add `IAttendanceDeductionSyncService attendanceSync` to the constructor signature and `_attendanceSync = attendanceSync;` in the body.

- [ ] **Step 4: Call sync at the top of `CalculateAsync`, before `ComputeAsync`**

In `CalculateAsync`, after the frozen population is loaded (`:114-116`) and **before** `var computation = await _computation.ComputeAsync(...)` (`:117`), insert:

```csharp
        // 2D: materialize attendance penalties into Approved deduction records for the frozen population so
        // they are consumed by the computation below (guaranteed even if "Sync Now" was never run).
        await _attendanceSync.SyncAsync(version, period, frozen, ct);
```

- [ ] **Step 5: Verify build + full suite**

Run: `cd backend && dotnet build && dotnet test tests/HR.Domain.Finance.Tests`
Expected: build 0 errors; all tests pass. (Any existing run-engine test that constructs `PayrollRunEngine` directly must be updated to pass an `IAttendanceDeductionSyncService` — construct a real `AttendanceDeductionSyncService` as in Step 1. Search: `grep -rn "new PayrollRunEngine(" backend/tests`.)

- [ ] **Step 6: Commit**

```bash
git add backend/src/HR.Infrastructure/Engines/Finance/PayrollRunEngine.cs \
        backend/tests/HR.Domain.Finance.Tests/AttendanceDeductionRunTests.cs
git commit -m "feat(payroll-2d): guarantee attendance deductions at Calculate (sync before compute)"
```

---

## Task 5: Retire `ATTENDANCE_DED` + seed `LATE`/`SHORTAGE` types

**Files:**
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/StandardPayrollSeeder.cs:27-39` (remove rule) and `:121-135` (neutralize step)
- Modify: `backend/src/HR.Infrastructure/Persistence/MasterDataDefaults.cs:73-77` (add two types)
- Test: `backend/tests/HR.Domain.Finance.Tests/StandardPayrollSeederAttendanceTests.cs`

**Interfaces:**
- Consumes: `Rule`, `FinanceRules` DbSet, `RuleSetVersion`.
- Produces: no `ATTENDANCE_DED` rule seeded for new tenants; existing active `ATTENDANCE_DED` rules deactivated (`IsActive = false`).

- [ ] **Step 1: Write the failing test** — `StandardPayrollSeederAttendanceTests.cs`

```csharp
using FluentAssertions;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class StandardPayrollSeederAttendanceTests
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

    [Fact]
    public async Task New_tenant_seed_does_not_create_attendance_ded_rule()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        await new StandardPayrollSeeder(db).EnsureStandardMonthlyAsync();

        (await db.FinanceRules.AnyAsync(r => r.Code == "ATTENDANCE_DED")).Should().BeFalse();
    }

    [Fact]
    public async Task Existing_attendance_ded_rule_is_deactivated_on_reseed()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        await new StandardPayrollSeeder(db).EnsureStandardMonthlyAsync();
        // Simulate a legacy tenant that already had the rule.
        var versionId = await db.FinanceRuleSetVersions.Select(v => v.Id).FirstAsync();
        db.FinanceRules.Add(new Rule
        {
            RuleSetVersionId = versionId, Code = "ATTENDANCE_DED", Name = "ATTENDANCE_DED", NameAr = "x",
            Kind = PayComponentKind.Deduction, Sequence = 99, ExpressionText = "0",
            ExpressionAstJson = "{}", OutputComponentCode = "ATTENDANCE_DED", IsActive = true,
        });
        await db.SaveChangesAsync();

        await new StandardPayrollSeeder(db).EnsureStandardMonthlyAsync();

        (await db.FinanceRules.Where(r => r.Code == "ATTENDANCE_DED").AllAsync(r => !r.IsActive))
            .Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter StandardPayrollSeederAttendanceTests`
Expected: FAIL — the seeder still adds `ATTENDANCE_DED` (first test fails) and never deactivates it (second fails).

- [ ] **Step 3: Remove the rule from the seeder array**

In `StandardPayrollSeeder.cs`, delete the `ATTENDANCE_DED` entry (`:34-38`) from the `Rules` array, and update the comment on `:24-26` to drop the attendance-input note. The array ends at `DEDUCTIONS`:

```csharp
    private static readonly RuleDef[] Rules =
    {
        new("BASIC", "الراتب الأساسي", PayComponentKind.Earning, "BasicSalary"),
        new("ALLOWANCES", "البدلات", PayComponentKind.Earning, "TotalAllowances"),
        new("ADDITIONS", "الإضافات", PayComponentKind.Earning, "TotalAdditions"),
        new("GOSI", "التأمينات الاجتماعية", PayComponentKind.Deduction, "ROUND(PERCENT(GosiBase, GosiRate), 2)"),
        new("DEDUCTIONS", "الاستقطاعات", PayComponentKind.Deduction, "TotalDeductions"),
    };
```

- [ ] **Step 4: Add the neutralize step to `EnsureRulesPresentAsync`**

In `StandardPayrollSeeder.cs`, at the end of `EnsureRulesPresentAsync` (after the `foreach` loop that adds missing rules, `:130-134`), append:

```csharp
        // 2D: retire the fact-based attendance rule — attendance deductions are now visible records.
        // Deactivate (never delete) any legacy ATTENDANCE_DED rule so historical runs stay auditable.
        var legacy = await _db.FinanceRules
            .Where(r => r.RuleSetVersionId == ruleSetVersionId && r.Code == "ATTENDANCE_DED" && r.IsActive)
            .ToListAsync(ct);
        foreach (var r in legacy) r.IsActive = false;
```

- [ ] **Step 5: Seed the two new deduction types**

In `backend/src/HR.Infrastructure/Persistence/MasterDataDefaults.cs`, extend the `DeductionType` block (`:73-77`):

```csharp
        Add(MasterDataObjectType.DeductionType,
            ("GOSI", "GOSI", "التأمينات الاجتماعية"),
            ("LOAN", "Loan Repayment", "سداد قرض"),
            ("ABSENCE", "Absence Deduction", "خصم غياب"),
            ("PENALTY", "Penalty", "جزاء"),
            ("LATE", "Late Arrival Deduction", "خصم تأخير"),
            ("SHORTAGE", "Working Hours Shortage", "خصم نقص ساعات العمل"));
```

- [ ] **Step 6: Run tests to verify they pass**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter StandardPayrollSeederAttendanceTests`
Expected: PASS. Then run the full finance suite to catch any seeder-dependent test: `dotnet test tests/HR.Domain.Finance.Tests`.

- [ ] **Step 7: Commit**

```bash
git add backend/src/HR.Infrastructure/Engines/Finance/StandardPayrollSeeder.cs \
        backend/src/HR.Infrastructure/Persistence/MasterDataDefaults.cs \
        backend/tests/HR.Domain.Finance.Tests/StandardPayrollSeederAttendanceTests.cs
git commit -m "feat(payroll-2d): retire ATTENDANCE_DED rule; seed LATE+SHORTAGE deduction types"
```

---

## Task 6: API surface — Sync Now endpoint + breakdown endpoint

**Files:**
- Modify: `backend/src/HR.Modules/Payroll/Controllers/PayrollController.cs` (constructor + two endpoints)
- Modify: `backend/src/HR.Modules/Payroll/DTOs/PayrollTransactionDtos.cs` (request/report/breakdown DTOs)

**Interfaces:**
- Consumes: `IAttendanceDeductionSyncService`, `AttendanceDeductionSyncReport`, `PayrollTransactionAttendanceReference`, `IScopeEngine`, `PayrollDefinitionVersion`.
- Produces:
  - `POST /api/payroll/attendance-deductions/sync` (perm `Payroll.Configure`), body `SyncAttendanceDeductionsRequest { Guid DefinitionId; int Year; int Month; List<Guid>? EmployeeIds }` → `AttendanceDeductionSyncReportDto`.
  - `GET /api/payroll/transactions/{id}/attendance-breakdown` (perm `Payroll.View`) → `List<AttendanceBreakdownDto>`.

- [ ] **Step 1: Add the DTOs** — append to `backend/src/HR.Modules/Payroll/DTOs/PayrollTransactionDtos.cs`

```csharp
public sealed class SyncAttendanceDeductionsRequest
{
    public Guid DefinitionId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public List<Guid>? EmployeeIds { get; set; }
}

public sealed record AttendanceDeductionSyncReportDto(
    int Created, int Updated, int Removed, int SkippedPosted, int TotalProcessed);

public sealed record AttendanceBreakdownDto(
    Guid AttendanceRecordId, DateTime Date, string PenaltyKind, int Minutes, int Days, decimal AmountContribution);
```

- [ ] **Step 2: Inject the sync service into the controller**

In `PayrollController.cs`, add a field `private readonly IAttendanceDeductionSyncService _attendanceSync;`, add `IAttendanceDeductionSyncService attendanceSync` to the constructor (`:39-42`), and assign `_attendanceSync = attendanceSync;`.

- [ ] **Step 3: Add the sync endpoint** — after the `transactions/impact-preview` endpoint (`:427`)

```csharp
    // ---- attendance deductions (2D) ----

    [HttpPost("attendance-deductions/sync")]
    [RequirePermission("Payroll.Configure")]
    public async Task<ActionResult<ApiResponse<AttendanceDeductionSyncReportDto>>> SyncAttendanceDeductions(
        [FromBody] SyncAttendanceDeductionsRequest req, CancellationToken ct)
    {
        var versionId = await ResolveVersionAsync(req.DefinitionId, ct);
        var version = await _db.PayrollDefinitionVersions.AsNoTracking().FirstOrDefaultAsync(v => v.Id == versionId, ct)
            ?? throw new NotFoundException("PayrollDefinitionVersion", versionId);

        IReadOnlyCollection<Guid> employeeIds = req.EmployeeIds is { Count: > 0 }
            ? req.EmployeeIds
            : (await _scope.ResolveAsync(SelectionScopeJson.Parse(version.SelectionScopeJson), ct)).IncludedEmployeeIds.ToList();

        var report = await _attendanceSync.SyncAsync(version, PayrollPeriod.Monthly(req.Year, req.Month), employeeIds, ct);
        return OkResponse(new AttendanceDeductionSyncReportDto(
            report.Created, report.Updated, report.Removed, report.SkippedPosted, report.TotalProcessed),
            $"Synced {report.TotalProcessed} attendance line(s).");
    }

    [HttpGet("transactions/{id:guid}/attendance-breakdown")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<List<AttendanceBreakdownDto>>>> AttendanceBreakdown(Guid id, CancellationToken ct)
    {
        var rows = await _db.PayrollTransactionAttendanceReferences.AsNoTracking()
            .Where(r => r.PayrollTransactionId == id)
            .OrderBy(r => r.Date)
            .Select(r => new AttendanceBreakdownDto(
                r.AttendanceRecordId, r.Date, r.PenaltyKind.ToString(), r.Minutes, r.Days, r.AmountContribution))
            .ToListAsync(ct);
        return OkResponse(rows);
    }
```

- [ ] **Step 4: Verify build**

Run: `cd backend && dotnet build`
Expected: 0 errors. (`OkResponse` has a `(value, message)` overload — confirm against `Bootstrap` at `:58`; if the two-arg form doesn't exist for this type, drop the message argument.)

- [ ] **Step 5: Commit**

```bash
git add backend/src/HR.Modules/Payroll/Controllers/PayrollController.cs \
        backend/src/HR.Modules/Payroll/DTOs/PayrollTransactionDtos.cs
git commit -m "feat(payroll-2d): Sync Now + attendance-breakdown endpoints"
```

---

## Task 7: Frontend — sync button, report, breakdown drawer

**Files:**
- Modify: the payroll deductions page + its api client. **First locate them:** `grep -rln "deductions" src/app` and `grep -rln "attendance-deductions\|transactions/impact-preview\|/payroll/transactions" src/lib src/app` to find the existing payroll transactions client (2A/2C added `/payroll/transactions` calls). Follow that client's exact patterns.

**Interfaces:**
- Consumes: `POST /api/payroll/attendance-deductions/sync`, `GET /api/payroll/transactions/{id}/attendance-breakdown`.

- [ ] **Step 1: Add API client functions** (mirror the existing payroll transaction client)

```ts
// in the existing payroll api client module
export interface AttendanceDeductionSyncReport {
  created: number; updated: number; removed: number; skippedPosted: number; totalProcessed: number;
}
export interface AttendanceBreakdownRow {
  attendanceRecordId: string; date: string; penaltyKind: string; minutes: number; days: number; amountContribution: number;
}

export async function syncAttendanceDeductions(body: {
  definitionId: string; year: number; month: number; employeeIds?: string[];
}): Promise<AttendanceDeductionSyncReport> {
  const res = await apiPost("/payroll/attendance-deductions/sync", body); // use the project's api helper
  return res.data;
}
export async function getAttendanceBreakdown(transactionId: string): Promise<AttendanceBreakdownRow[]> {
  const res = await apiGet(`/payroll/transactions/${transactionId}/attendance-breakdown`);
  return res.data;
}
```

- [ ] **Step 2: Add the "Sync attendance deductions" button + period selector** to the deductions page

- The button posts `{ definitionId, year, month }` (definition = the standard MONTHLY payroll; reuse however the page already picks a definition/period). On success show a toast summarizing the report: `Created ${r.created}, Updated ${r.updated}, Removed ${r.removed}, Skipped ${r.skippedPosted}`.
- After sync, refresh the deductions list.

- [ ] **Step 3: Render attendance-sourced rows as read-only with a breakdown drawer**

- For each transaction row where `sourceModule === "Attendance"`, hide edit/delete; render a "Breakdown" action that opens a drawer.
- On open, call `getAttendanceBreakdown(txn.id)` and render a table: Date · Kind · Minutes/Days · Amount, with a footer total. Add the guidance line: *"Attendance-driven — correct the attendance record and re-sync."*

- [ ] **Step 4: Verify the frontend build**

Run: `npm run build` (from repo root)
Expected: build succeeds. Verify the deductions page renders and the sync button calls the endpoint (manual smoke test against a local/preview API if available).

- [ ] **Step 5: Commit**

```bash
git add src/
git commit -m "feat(payroll-2d): attendance deduction sync button + breakdown drawer (read-only rows)"
```

---

## Task 8: Full verification + migration/deploy note

**Files:** none (verification only).

- [ ] **Step 1: Backend build + full test run**

Run: `cd backend && dotnet build && dotnet test`
Expected: build 0 errors; all test projects pass (finance suite includes the 4 new test files).

- [ ] **Step 2: Confirm the migration is the only schema change**

Run: `cd backend/src/HR.Infrastructure && dotnet ef migrations has-pending-model-changes --startup-project ../HR.Api`
Expected: reports no pending model changes (the `AttendanceDeductionReference` migration from Task 1 already captures the one new table; `PayrollTransaction`/ledger are unchanged).

- [ ] **Step 3: Frontend build**

Run: `npm run build`
Expected: succeeds.

- [ ] **Step 4: Record the deploy checklist (do NOT deploy without user authorization)**

Deployment (user-authorized, mirrors 2C):
1. `dotnet ef database update --startup-project ../HR.Api` against Azure Postgres (applies `engine_payroll_transaction_attendance_refs`).
2. Re-run payroll bootstrap per tenant (`POST /api/payroll/bootstrap`) to seed `LATE`/`SHORTAGE` types **and** deactivate any legacy `ATTENDANCE_DED` rule.
3. Publish + zip-deploy the API to `hrcloud-api-v4xd` (use the `ZipFile` + `.Replace('\\','/')` method from CLAUDE.md, not `Compress-Archive`).
4. Vercel auto-deploys the frontend.
5. Live-verify: create attendance penalties → Sync Now returns a report → the deduction appears in `/payroll/deductions` with a working breakdown drawer → a run's Calculate consumes it once (no double count) → Execute posts one ledger entry.

- [ ] **Step 5: Final commit (if any verification fixes were needed)**

```bash
git add -A
git commit -m "chore(payroll-2d): verification pass — full build + tests green"
```

---

## Self-Review Notes (coverage against the spec)

- Spec §4.1 enum → **T1**. §4.2 shared calculator → **T2** (fact provider reuses; wages read from provider = zero drift). §4.3 sync service (born Approved, upsert, remove-on-zero, skip-posted, toggle) → **T3**. §4.4 reference table + migration → **T1**. §4.5 sync report → **T3** + surfaced in **T6/T7**. §4.6 retire rule + seed types → **T5**. §4.7 two call sites → Calculate wiring **T4**, endpoint **T6**. §5 frontend → **T7**. §6 DomainException on missing type → **T3**. §7 tests 1–8 → distributed across T2/T3/T4/T5. §8 one migration → **T1**. §9 acceptance → verified in **T8**.
- Type consistency: `AttendanceDeductionSyncReport`, `AttendancePenaltyKind`, `AttendanceAggregate`, `AttendanceBreakdownRow`, `PayrollTransactionAttendanceReference` names are identical across all tasks.
- Open verification points flagged inline for the implementer: exact `DomainException` namespace (T3 S7), `OkResponse` two-arg overload (T6 S4), any direct `new PayrollRunEngine(` in tests (T4 S5), and the frontend deductions page path (T7).

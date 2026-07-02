# Payroll Sub-project 2E — Attendance Daily Actions + Overtime + Configurable Rates + Excuse Trigger

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extend the live 2D attendance→payroll engine so overtime becomes a visible paid Addition, all attendance rates are configurable, HR can materialize an employee's attendance payroll impact per-month from the daily attendance page, and an approved excuse/leave cancels the related deduction by fixing the attendance record at its source.

**Architecture:** Generalize 2D's `AttendanceDeductionSyncService` to a 4th kind (Overtime → Addition) and rename it `AttendancePayrollSyncService`; read configurable multipliers from `CalcSettingsJson`; add a per-employee/month sync endpoint on the Attendance controller; fix the correction/leave completion executors to zero penalty minutes so 2D's cancel-on-zero fires.

**Tech Stack:** .NET 8, EF Core 8 + Npgsql, xUnit + FluentAssertions + EF InMemory (tests), Next.js (App Router) frontend.

## Global Constraints

- **Target framework:** `net8.0` for all projects.
- **Persistence:** Npgsql/PostgreSQL; all persisted `DateTime` must be `DateTimeKind.Utc`.
- **Tenant + audit implicit:** inject the concrete `ApplicationDbContext`; it stamps tenant/audit in `SaveChangesAsync`. Do not inject `ICurrentUserService` in the sync service.
- **Reuse 2D exactly:** the upsert key `(EmployeeId, TargetPeriodYear, TargetPeriodMonth, TypeId, SourceModule="Attendance")`, born `Approved`, cancel-on-zero, skip `Posted`/`Reversed`, `PayrollTransactionAttendanceReference` drill-down — all unchanged in behavior.
- **Rate defaults preserve 2D exactly:** absence/late/shortage multipliers default `1.0`; overtime `1.5`. Absent config ⇒ current amounts unchanged.
- **Overtime is opt-in:** `includeOvertime` defaults **false**; the daily "Calculate overtime addition" action passes an explicit `includeOvertime=true` override.
- **Business-rule errors** throw `DomainException` (422); it lives in `HR.Application.Common.Exceptions`.
- **No schema change** beyond one `HasData` seed-data migration for the new permission `Attendance.PayrollImpact.Create`.
- **`Employee` is in `HR.Modules.Employees.Entities`** (not `HR.Domain.Entities`). Master-data object types are `MasterDataObjectType.DeductionType` / `.AdditionType` (const strings) in `HR.Domain.Engines.MasterData`.
- **TDD, frequent commits.** Finance tests in `backend/tests/HR.Domain.Finance.Tests` (EF InMemory, `FakeUser : ICurrentUserService`, unique DB per test, manual service construction).

---

## File Structure

**Domain**
- Modify `src/HR.Domain/Enums/AttendancePenaltyKind.cs` → rename enum to `AttendancePayrollKind`, add `Overtime = 4`.
- Modify `src/HR.Domain/Engines/Finance/Entities/PayrollTransactionAttendanceReference.cs` — `PenaltyKind` property type follows the rename.

**Application**
- Rename `src/HR.Application/Engines/Finance/IAttendanceDeductionSyncService.cs` → `IAttendancePayrollSyncService.cs` (interface `IAttendancePayrollSyncService`, report `AttendancePayrollSyncReport`, add optional `bool? includeOvertime` param).

**Infrastructure**
- Rename `src/HR.Infrastructure/Engines/Finance/AttendanceDeductionSyncService.cs` → `AttendancePayrollSyncService.cs`.
- Modify `src/HR.Infrastructure/Engines/Finance/PayrollCalcSettings.cs` — add `AttendanceRates` + `IncludeOvertime`.
- Modify `src/HR.Infrastructure/Engines/Finance/AttendanceWageCalculator.cs` — `BreakdownRowsAsync` emits Overtime rows.
- Modify `src/HR.Infrastructure/Engines/Finance/PayrollRunEngine.cs` — field/ctor/call renamed.
- Modify `src/HR.Infrastructure/DependencyInjection.cs` — DI registration renamed.
- Modify `src/HR.Infrastructure/Persistence/SeedData.cs` — add the permission.
- Create migration `src/HR.Infrastructure/Migrations/<ts>_AttendancePayrollImpactPermission.cs` (generated).

**Modules**
- Modify `src/HR.Modules/Payroll/Controllers/PayrollController.cs` — field/ctor/endpoint renamed types.
- Modify `src/HR.Modules/Payroll/DTOs/PayrollTransactionDtos.cs` — report DTO name follows rename (or keep; see Task 1).
- Modify `src/HR.Modules/Attendance/Completion/AttendanceCorrectionExecutor.cs` — zero penalty minutes.
- Modify `src/HR.Modules/Attendance/Completion/AttendanceApplyLeaveDaysExecutor.cs` — upsert + zero minutes.
- Modify `src/HR.Modules/Attendance/Controllers/AttendanceController.cs` — new payroll-impact endpoint + DTO.

**Tests**
- Modify existing 2D tests for the rename; add new tests per task.
- Modify `tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj` — add a `HR.Modules.Attendance` project reference (Task 4, to test the executors).

**Frontend**
- Modify `src/app/(dashboard)/attendance/page.tsx` (`DailyTable`) + `src/lib/api/attendance.ts`.

---

## Task 1: Rename the 2D sync engine + add the Overtime kind value

**Files:**
- Modify: `backend/src/HR.Domain/Enums/AttendancePenaltyKind.cs`
- Modify: `backend/src/HR.Application/Engines/Finance/IAttendanceDeductionSyncService.cs` (rename file → `IAttendancePayrollSyncService.cs`)
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/AttendanceDeductionSyncService.cs` (rename file → `AttendancePayrollSyncService.cs`)
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/PayrollRunEngine.cs`, `DependencyInjection.cs`, `PayrollController.cs`, `PayrollTransactionDtos.cs`, `PayrollTransactionAttendanceReference.cs`, `AttendanceWageCalculator.cs` (the `AttendanceBreakdownRow.PenaltyKind` type)
- Modify: all 2D test files referencing the old names.

**Interfaces:**
- Produces: `enum AttendancePayrollKind { Absence=1, Late=2, Shortage=3, Overtime=4 }`; `interface IAttendancePayrollSyncService`; `record AttendancePayrollSyncReport(int Created,int Updated,int Removed,int SkippedPosted,int TotalProcessed)`; `class AttendancePayrollSyncService`. `Overtime` is added to the enum but **not yet wired into the sync** (that's Task 3).

- [ ] **Step 1: Rename the enum + add Overtime**

Rewrite `backend/src/HR.Domain/Enums/AttendancePenaltyKind.cs` (keep the filename or rename to `AttendancePayrollKind.cs`; C# doesn't require the filename to match):

```csharp
namespace HR.Domain.Enums;

/// <summary>Business semantics for an attendance-driven payroll impact. The engine keys on this enum;
/// customer-configurable master-data items (DeductionType ABSENCE/LATE/SHORTAGE, AdditionType OVERTIME)
/// supply labels only.</summary>
public enum AttendancePayrollKind
{
    Absence = 1,
    Late = 2,
    Shortage = 3,
    Overtime = 4,
}
```

- [ ] **Step 2: Rename the interface + report + add the override param**

Rewrite `IAttendanceDeductionSyncService.cs` → content of `IAttendancePayrollSyncService.cs`:

```csharp
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;

namespace HR.Application.Engines.Finance;

/// <summary>Outcome of an attendance payroll sync pass.</summary>
public sealed record AttendancePayrollSyncReport(int Created, int Updated, int Removed, int SkippedPosted, int TotalProcessed);

/// <summary>Materializes attendance impacts (absence/late/shortage deductions + overtime additions) into
/// Approved PayrollTransaction records — one per employee/period/kind — idempotently. Single source of
/// truth for attendance payroll effects.</summary>
public interface IAttendancePayrollSyncService
{
    Task<AttendancePayrollSyncReport> SyncAsync(
        PayrollDefinitionVersion version, PayrollPeriod period,
        IReadOnlyCollection<Guid> employeeIds, bool? includeOvertime = null, CancellationToken ct = default);
}
```

Rename the file: `git mv backend/src/HR.Application/Engines/Finance/IAttendanceDeductionSyncService.cs backend/src/HR.Application/Engines/Finance/IAttendancePayrollSyncService.cs` (do this before editing, or create-new + delete-old).

- [ ] **Step 3: Rename the implementation (behavior unchanged for now)**

`git mv` `AttendanceDeductionSyncService.cs` → `AttendancePayrollSyncService.cs`. Change the class name to `AttendancePayrollSyncService`, implement `IAttendancePayrollSyncService`, rename the return type to `AttendancePayrollSyncReport`, add the `bool? includeOvertime = null` parameter to `SyncAsync` (accept it but ignore it for now — Task 3 uses it), and replace every `AttendancePenaltyKind` with `AttendancePayrollKind`. Keep `KindCodes` as the 3 deduction kinds only (Overtime wired in Task 3). The `SyncAsync` signature becomes:

```csharp
    public async Task<AttendancePayrollSyncReport> SyncAsync(
        PayrollDefinitionVersion version, PayrollPeriod period,
        IReadOnlyCollection<Guid> employeeIds, bool? includeOvertime = null, CancellationToken ct = default)
```

Return `new AttendancePayrollSyncReport(...)` everywhere. Leave all other logic identical.

- [ ] **Step 4: Update all references**

- `PayrollTransactionAttendanceReference.cs`: property `public AttendancePayrollKind PenaltyKind { get; set; }`.
- `AttendanceWageCalculator.cs`: `AttendanceBreakdownRow(... AttendancePayrollKind PenaltyKind ...)` and the two `AttendancePenaltyKind.` usages → `AttendancePayrollKind.`.
- `DependencyInjection.cs:72`: `services.AddScoped<HR.Application.Engines.Finance.IAttendancePayrollSyncService, HR.Infrastructure.Engines.Finance.AttendancePayrollSyncService>();`
- `PayrollRunEngine.cs`: field `IAttendancePayrollSyncService _attendanceSync`, ctor param type, and the call `await _attendanceSync.SyncAsync(version, period, frozen, ct: ct);` (named `ct` because `includeOvertime` now precedes it).
- `PayrollController.cs`: if it injects/uses the sync service or its report DTO, rename the types. (The 2D sync endpoint uses `AttendanceDeductionSyncReportDto` — that's a separate controller DTO in `PayrollTransactionDtos.cs`; keep its name or rename to `AttendancePayrollSyncReportDto`. Rename for consistency and update both the controller mapping and the DTO.)
- Search and update: `grep -rn "AttendancePenaltyKind\|AttendanceDeductionSyncService\|IAttendanceDeductionSyncService\|AttendanceDeductionSyncReport" backend` — fix every hit including tests.

- [ ] **Step 5: Run the full finance suite (rename regression)**

Run: `cd backend && dotnet build && dotnet test tests/HR.Domain.Finance.Tests`
Expected: build 0 errors; all tests pass (the rename is behavior-preserving; `includeOvertime` defaults null and is ignored).

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "refactor(payroll-2e): rename AttendanceDeductionSync→AttendancePayrollSync; add AttendancePayrollKind.Overtime"
```

---

## Task 2: Configurable attendance rates

**Files:**
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/PayrollCalcSettings.cs`
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/AttendancePayrollSyncService.cs` (apply multipliers)
- Test: `backend/tests/HR.Domain.Finance.Tests/PayrollCalcSettingsTests.cs` + extend the sync tests

**Interfaces:**
- Produces: `readonly record struct AttendanceRates(decimal Absence, decimal Late, decimal Shortage, decimal Overtime)`; `PayrollCalcSettings.Rates(string?) → AttendanceRates` (defaults 1/1/1/1.5); `PayrollCalcSettings.IncludeOvertime(string?) → bool` (default false).

- [ ] **Step 1: Write the failing tests** — `PayrollCalcSettingsTests.cs`

```csharp
using FluentAssertions;
using HR.Infrastructure.Engines.Finance;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollCalcSettingsTests
{
    [Fact]
    public void Rates_default_when_absent()
    {
        var r = PayrollCalcSettings.Rates(null);
        r.Absence.Should().Be(1.0m); r.Late.Should().Be(1.0m); r.Shortage.Should().Be(1.0m); r.Overtime.Should().Be(1.5m);
    }

    [Fact]
    public void Rates_read_overrides()
    {
        var json = "{\"attendanceRates\":{\"absenceMultiplier\":2,\"lateMultiplier\":1.25,\"shortageMultiplier\":0.5,\"overtimeMultiplier\":2}}";
        var r = PayrollCalcSettings.Rates(json);
        r.Absence.Should().Be(2m); r.Late.Should().Be(1.25m); r.Shortage.Should().Be(0.5m); r.Overtime.Should().Be(2m);
    }

    [Fact]
    public void IncludeOvertime_defaults_false_and_reads_true()
    {
        PayrollCalcSettings.IncludeOvertime(null).Should().BeFalse();
        PayrollCalcSettings.IncludeOvertime("{\"includeOvertime\":true}").Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run to verify fail**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter PayrollCalcSettingsTests`
Expected: FAIL — `Rates`/`IncludeOvertime`/`AttendanceRates` do not exist.

- [ ] **Step 3: Extend `PayrollCalcSettings`**

Append to `PayrollCalcSettings.cs`:

```csharp
    public readonly record struct AttendanceRates(decimal Absence, decimal Late, decimal Shortage, decimal Overtime);

    private static readonly AttendanceRates Defaults = new(1.0m, 1.0m, 1.0m, 1.5m);

    /// <summary>Reads attendanceRates multipliers; any missing/invalid value falls back to its statutory default.</summary>
    public static AttendanceRates Rates(string? calcSettingsJson)
    {
        if (string.IsNullOrWhiteSpace(calcSettingsJson)) return Defaults;
        try
        {
            using var doc = JsonDocument.Parse(calcSettingsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object
                || !doc.RootElement.TryGetProperty("attendanceRates", out var r)
                || r.ValueKind != JsonValueKind.Object)
                return Defaults;
            return new AttendanceRates(
                Mult(r, "absenceMultiplier", Defaults.Absence),
                Mult(r, "lateMultiplier", Defaults.Late),
                Mult(r, "shortageMultiplier", Defaults.Shortage),
                Mult(r, "overtimeMultiplier", Defaults.Overtime));
        }
        catch (JsonException) { return Defaults; }
    }

    /// <summary>Whether overtime additions are materialized. Opt-in: default false.</summary>
    public static bool IncludeOvertime(string? calcSettingsJson)
    {
        if (string.IsNullOrWhiteSpace(calcSettingsJson)) return false;
        try
        {
            using var doc = JsonDocument.Parse(calcSettingsJson);
            return doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("includeOvertime", out var v)
                && v.ValueKind == JsonValueKind.True;
        }
        catch (JsonException) { return false; }
    }

    private static decimal Mult(JsonElement obj, string key, decimal fallback)
        => obj.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetDecimal(out var d)
            ? d : fallback;
```

- [ ] **Step 4: Apply the deduction multipliers in the sync service**

In `AttendancePayrollSyncService.SyncAsync`, after reading facts, read the rates once and multiply the deduction amounts. Change the amount switch:

```csharp
            var rates = PayrollCalcSettings.Rates(version.CalcSettingsJson);
```
(place this near the top of `SyncAsync`, before the employee loop) and inside the loop:

```csharp
                var amount = kind switch
                {
                    AttendancePayrollKind.Absence => Math.Round(Dec(f, "AbsentDays") * dailyWage * rates.Absence, 2),
                    AttendancePayrollKind.Late => Math.Round(Dec(f, "LateHours") * hourlyWage * rates.Late, 2),
                    AttendancePayrollKind.Shortage => Math.Round(Dec(f, "ShortageHours") * hourlyWage * rates.Shortage, 2),
                    _ => 0m,
                };
```

Defaults (1.0) keep 2D amounts identical.

- [ ] **Step 5: Run tests**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter PayrollCalcSettingsTests`
Then the full suite: `dotnet test tests/HR.Domain.Finance.Tests`
Expected: PASS; existing 2D deduction-amount tests still pass (×1.0 unchanged).

- [ ] **Step 6: Commit**

```bash
git add backend/src/HR.Infrastructure/Engines/Finance/PayrollCalcSettings.cs \
        backend/src/HR.Infrastructure/Engines/Finance/AttendancePayrollSyncService.cs \
        backend/tests/HR.Domain.Finance.Tests/PayrollCalcSettingsTests.cs
git commit -m "feat(payroll-2e): configurable attendance rate multipliers (defaults preserve 2D)"
```

---

## Task 3: Overtime addition kind

**Files:**
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/AttendancePayrollSyncService.cs`
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/AttendanceWageCalculator.cs` (`BreakdownRowsAsync` emits Overtime rows)
- Test: `backend/tests/HR.Domain.Finance.Tests/AttendanceOvertimeSyncTests.cs`

**Interfaces:**
- Consumes: `PayrollCalcSettings.IncludeOvertime`, `PayrollCalcSettings.Rates`, `MasterDataObjectType.AdditionType`, `PayrollTransactionKind.Addition`.
- Produces: overtime materialized as `PayrollTransaction { Kind=Addition, TypeId=OVERTIME AdditionType, SourceModule="Attendance" }`, gated by `includeOvertime ?? config`.

- [ ] **Step 1: Write the failing test** — `AttendanceOvertimeSyncTests.cs`

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

namespace HR.Domain.Finance.Tests;

public class AttendanceOvertimeSyncTests
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

    private static AttendancePayrollSyncService Svc(ApplicationDbContext db)
    {
        var facts = new PayrollFactProvider(db, null!, new AttendanceWageCalculator(db));
        return new AttendancePayrollSyncService(db, facts, new AttendanceWageCalculator(db));
    }

    private static async Task<Guid> SeedAsync(ApplicationDbContext db)
    {
        // Fixed30 basis: dailyWage = 2400/30 = 80, hourlyWage = 10.
        var emp = new Employee { EmployeeNumber = "E1", FirstName = "Ali", LastName = "S", Email = "a@t.local", BasicSalary = 2400m };
        db.Employees.Add(emp);
        foreach (var (obj, code) in new[] {
            (MasterDataObjectType.DeductionType, "ABSENCE"), (MasterDataObjectType.DeductionType, "LATE"),
            (MasterDataObjectType.DeductionType, "SHORTAGE"), (MasterDataObjectType.AdditionType, "OVERTIME") })
            db.MasterDataItems.Add(new MasterDataItem { ObjectType = obj, Code = code, NameAr = code, NameEn = code });
        await db.SaveChangesAsync();
        return emp.Id;
    }

    private static PayrollDefinitionVersion Version(string? calc = null) => new()
    { DayBasis = DayBasis.Fixed30, CutoffDay = 27, CarryToNextPeriod = true, Currency = "SAR", CalcSettingsJson = calc };

    [Fact]
    public async Task Overtime_creates_addition_at_1_5x_when_requested()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = await SeedAsync(db);
        db.AttendanceRecords.Add(new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,3), Status = AttendanceStatus.Present, OvertimeMinutes = 120 });
        await db.SaveChangesAsync();

        var report = await Svc(db).SyncAsync(Version(), new PayrollPeriod(Utc(2026,7,1), Utc(2026,7,31)), new[] { emp }, includeOvertime: true);

        var ot = await db.PayrollTransactions.SingleAsync(t => t.SourceModule == "Attendance" && t.Kind == PayrollTransactionKind.Addition);
        ot.Status.Should().Be(PayrollTransactionStatus.Approved);
        ot.Amount.Should().Be(30m); // 2 hrs * hourlyWage 10 * 1.5
        report.Created.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Overtime_not_created_when_opted_out()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = await SeedAsync(db);
        db.AttendanceRecords.Add(new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,3), Status = AttendanceStatus.Present, OvertimeMinutes = 120 });
        await db.SaveChangesAsync();

        // includeOvertime null → config default (false)
        await Svc(db).SyncAsync(Version(), new PayrollPeriod(Utc(2026,7,1), Utc(2026,7,31)), new[] { emp });

        (await db.PayrollTransactions.AnyAsync(t => t.Kind == PayrollTransactionKind.Addition)).Should().BeFalse();
    }

    [Fact]
    public async Task Overtime_cancels_on_zero()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = await SeedAsync(db);
        var rec = new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,3), Status = AttendanceStatus.Present, OvertimeMinutes = 120 };
        db.AttendanceRecords.Add(rec);
        await db.SaveChangesAsync();
        var period = new PayrollPeriod(Utc(2026,7,1), Utc(2026,7,31));
        await Svc(db).SyncAsync(Version(), period, new[] { emp }, includeOvertime: true);

        rec.OvertimeMinutes = 0;
        await db.SaveChangesAsync();
        await Svc(db).SyncAsync(Version(), period, new[] { emp }, includeOvertime: true);

        (await db.PayrollTransactions.Single(t => t.Kind == PayrollTransactionKind.Addition).Status)
            .Should().Be(PayrollTransactionStatus.Cancelled);
    }
}
```

- [ ] **Step 2: Run to verify fail**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter AttendanceOvertimeSyncTests`
Expected: FAIL — overtime not wired; no Addition produced.

- [ ] **Step 3: Wire the Overtime kind into the sync service**

In `AttendancePayrollSyncService.cs`, replace the `KindCodes` array + `ResolveTypesAsync` + the per-kind gating + the create `Kind`. Full changes:

Replace the `KindCodes` field:
```csharp
    private static readonly (AttendancePayrollKind Kind, string Code, string ObjectType, PayrollTransactionKind TxnKind)[] KindSpecs =
    {
        (AttendancePayrollKind.Absence,  "ABSENCE",  MasterDataObjectType.DeductionType, PayrollTransactionKind.Deduction),
        (AttendancePayrollKind.Late,     "LATE",     MasterDataObjectType.DeductionType, PayrollTransactionKind.Deduction),
        (AttendancePayrollKind.Shortage, "SHORTAGE", MasterDataObjectType.DeductionType, PayrollTransactionKind.Deduction),
        (AttendancePayrollKind.Overtime, "OVERTIME", MasterDataObjectType.AdditionType,  PayrollTransactionKind.Addition),
    };
```

Replace the top-of-`SyncAsync` guard + rates read:
```csharp
        var includeDed = PayrollCalcSettings.IncludeAttendanceDeductions(version.CalcSettingsJson);
        var includeOt = includeOvertime ?? PayrollCalcSettings.IncludeOvertime(version.CalcSettingsJson);
        if (employeeIds.Count == 0 || (!includeDed && !includeOt))
            return new AttendancePayrollSyncReport(0, 0, 0, 0, 0);

        var rates = PayrollCalcSettings.Rates(version.CalcSettingsJson);
        var typeByKind = await ResolveTypesAsync(ct); // now returns (Guid TypeId, PayrollTransactionKind TxnKind)
```

Rewrite `ResolveTypesAsync`:
```csharp
    private async Task<IReadOnlyDictionary<AttendancePayrollKind, (Guid TypeId, PayrollTransactionKind TxnKind)>> ResolveTypesAsync(CancellationToken ct)
    {
        var found = await _db.MasterDataItems.AsNoTracking()
            .Where(m => (m.ObjectType == MasterDataObjectType.DeductionType || m.ObjectType == MasterDataObjectType.AdditionType))
            .Select(m => new { m.ObjectType, m.Code, m.Id }).ToListAsync(ct);
        var byKey = found.ToDictionary(x => (x.ObjectType, x.Code.ToUpperInvariant()), x => x.Id);
        var map = new Dictionary<AttendancePayrollKind, (Guid, PayrollTransactionKind)>();
        foreach (var spec in KindSpecs)
        {
            if (!byKey.TryGetValue((spec.ObjectType, spec.Code), out var id))
                throw new DomainException($"No {spec.ObjectType} configured for attendance kind '{spec.Kind}' (code {spec.Code}). Re-run payroll bootstrap.");
            map[spec.Kind] = (id, spec.TxnKind);
        }
        return map;
    }
```

Rewrite the per-kind loop (gating + amount + create Kind). Replace the `foreach (var (kind, code) in KindCodes)` block with:
```csharp
            foreach (var spec in KindSpecs)
            {
                var kind = spec.Kind;
                var enabled = kind == AttendancePayrollKind.Overtime ? includeOt : includeDed;
                if (!enabled) continue;
                processed++;

                var amount = kind switch
                {
                    AttendancePayrollKind.Absence  => Math.Round(Dec(f, "AbsentDays")   * dailyWage  * rates.Absence, 2),
                    AttendancePayrollKind.Late     => Math.Round(Dec(f, "LateHours")     * hourlyWage * rates.Late, 2),
                    AttendancePayrollKind.Shortage => Math.Round(Dec(f, "ShortageHours") * hourlyWage * rates.Shortage, 2),
                    AttendancePayrollKind.Overtime => Math.Round(Dec(f, "OvertimeHours") * hourlyWage * rates.Overtime, 2),
                    _ => 0m,
                };
                var (typeId, txnKind) = typeByKind[kind];
                existingByKey.TryGetValue((empId, typeId), out var txn);

                if (txn is { Status: PayrollTransactionStatus.Posted } or { Status: PayrollTransactionStatus.Reversed })
                { skipped++; continue; }

                if (amount <= 0m)
                {
                    if (txn is not null && txn.Status != PayrollTransactionStatus.Cancelled)
                    {
                        txn.Status = PayrollTransactionStatus.Cancelled;
                        txn.StatusReason = "Attendance impact cleared on re-sync.";
                        await ClearRefsAsync(txn.Id, ct);
                        removed++;
                    }
                    continue;
                }

                if (txn is null)
                {
                    txn = new PayrollTransaction
                    {
                        Kind = txnKind,
                        EmployeeId = empId,
                        TypeId = typeId,
                        Amount = amount,
                        EffectiveDate = period.Start,
                        TransactionDate = period.End,
                        TargetPeriodYear = period.Year,
                        TargetPeriodMonth = period.Month,
                        SourceModule = Source,
                        ReferenceType = RefType,
                        Status = PayrollTransactionStatus.Approved,
                    };
                    _db.PayrollTransactions.Add(txn);
                    await _db.SaveChangesAsync(ct);
                    created++;
                }
                else
                {
                    txn.Amount = amount;
                    if (txn.Status == PayrollTransactionStatus.Cancelled)
                        txn.Status = PayrollTransactionStatus.Approved;
                    updated++;
                }

                var rateForKind = kind switch
                {
                    AttendancePayrollKind.Absence => rates.Absence, AttendancePayrollKind.Late => rates.Late,
                    AttendancePayrollKind.Shortage => rates.Shortage, _ => rates.Overtime,
                };
                await WriteRefsAsync(txn.Id, kind, dailyWage, hourlyWage, rateForKind,
                    rowsByEmp.TryGetValue(empId, out var rs) ? rs : new(), ct);
            }
```

Update `WriteRefsAsync` to take the rate multiplier and price overtime rows:
```csharp
    private async Task WriteRefsAsync(Guid txnId, AttendancePayrollKind kind, decimal dailyWage, decimal hourlyWage,
        decimal multiplier, List<AttendanceBreakdownRow> rows, CancellationToken ct)
    {
        await ClearRefsAsync(txnId, ct);
        foreach (var r in rows.Where(r => r.PenaltyKind == kind))
        {
            var contribution = kind == AttendancePayrollKind.Absence
                ? dailyWage * r.Days * multiplier
                : Math.Round(r.Minutes / 60m, 2) * hourlyWage * multiplier;
            _db.PayrollTransactionAttendanceReferences.Add(new PayrollTransactionAttendanceReference
            {
                PayrollTransactionId = txnId, AttendanceRecordId = r.AttendanceRecordId,
                Date = r.Date, PenaltyKind = kind, Minutes = r.Minutes, Days = r.Days,
                AmountContribution = contribution,
            });
        }
    }
```

- [ ] **Step 4: Emit Overtime rows from `BreakdownRowsAsync`**

In `AttendanceWageCalculator.cs`, in `BreakdownRowsAsync`, after the Late/Shortage row emission for a non-absent day, add:
```csharp
            if (d.OvertimeMinutes > 0)
                rows.Add(new AttendanceBreakdownRow(d.EmployeeId, d.Id, d.Date, AttendancePayrollKind.Overtime, d.OvertimeMinutes, 0));
```
Ensure the projection selects `OvertimeMinutes` (add `a.OvertimeMinutes` to the `.Select(...)` if not already present).

- [ ] **Step 5: Run tests**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter AttendanceOvertimeSyncTests`
Then full suite: `dotnet test tests/HR.Domain.Finance.Tests`
Expected: PASS. Existing 2D deduction tests still green (deduction path unchanged except the ×multiplier which defaults to 1.0). NOTE: 2D tests that call `SyncAsync(...)` without `includeOvertime` still compile (defaulted) and behave identically (overtime off by default).

- [ ] **Step 6: Commit**

```bash
git add backend/src/HR.Infrastructure/Engines/Finance/AttendancePayrollSyncService.cs \
        backend/src/HR.Infrastructure/Engines/Finance/AttendanceWageCalculator.cs \
        backend/tests/HR.Domain.Finance.Tests/AttendanceOvertimeSyncTests.cs
git commit -m "feat(payroll-2e): overtime materialized as Addition (opt-in, configurable multiplier)"
```

---

## Task 4: Excuse/leave executors zero penalty minutes

**Files:**
- Modify: `backend/src/HR.Modules/Attendance/Completion/AttendanceCorrectionExecutor.cs`
- Modify: `backend/src/HR.Modules/Attendance/Completion/AttendanceApplyLeaveDaysExecutor.cs`
- Modify: `backend/tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj` (add project reference)
- Test: `backend/tests/HR.Domain.Finance.Tests/AttendanceExcuseExecutorTests.cs`

**Interfaces:**
- Consumes: `EffectContext` (`HR.Application.Engines.Completion`), `AttendanceRecord`, `AttendanceStatus`.
- Produces: excused/leave days carry zero `LateMinutes`/`ShortageMinutes` → 2D cancel-on-zero fires.

- [ ] **Step 1: Add the project reference so executors are testable**

In `backend/tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj`, add to the `<ItemGroup>` of project references:
```xml
    <ProjectReference Include="..\..\src\HR.Modules\Attendance\HR.Modules.Attendance.csproj" />
```

- [ ] **Step 2: Write the failing tests** — `AttendanceExcuseExecutorTests.cs`

```csharp
using System.Text.Json;
using FluentAssertions;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Completion;
using HR.Domain.Engines.Attendance;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Attendance.Completion;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class AttendanceExcuseExecutorTests
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

    private static EffectContext Effctx(Guid emp, JsonElement payload) => new()
    { RequestInstanceId = Guid.NewGuid(), RequestNumber = "R1", RequestTypeCode = "X", EmployeeId = emp, Payload = payload };

    [Fact]
    public async Task Correction_sets_present_and_zeroes_penalty_minutes()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = Guid.NewGuid();
        db.AttendanceRecords.Add(new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,5), Status = AttendanceStatus.Late, LateMinutes = 45, ShortageMinutes = 20 });
        await db.SaveChangesAsync();

        var payload = JsonDocument.Parse("{\"date\":\"2026-07-05\",\"reason\":\"excused\"}").RootElement;
        await new AttendanceCorrectionExecutor(db).ExecuteAsync(Effctx(emp, payload), default);

        var rec = await db.AttendanceRecords.SingleAsync(a => a.EmployeeId == emp);
        rec.Status.Should().Be(AttendanceStatus.Present);
        rec.LateMinutes.Should().Be(0);
        rec.ShortageMinutes.Should().Be(0);
    }

    [Fact]
    public async Task Leave_upserts_existing_day_and_zeroes_minutes_no_duplicate()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = Guid.NewGuid();
        db.AttendanceRecords.Add(new AttendanceRecord { EmployeeId = emp, Date = Utc(2026,7,5), Status = AttendanceStatus.Late, LateMinutes = 30 });
        await db.SaveChangesAsync();

        var payload = JsonDocument.Parse("{\"startDate\":\"2026-07-05\",\"endDate\":\"2026-07-05\"}").RootElement;
        await new AttendanceApplyLeaveDaysExecutor(db).ExecuteAsync(Effctx(emp, payload), default);

        var recs = await db.AttendanceRecords.Where(a => a.EmployeeId == emp && a.Date == Utc(2026,7,5)).ToListAsync();
        recs.Should().HaveCount(1); // upsert, not duplicate
        recs[0].Status.Should().Be(AttendanceStatus.OnLeave);
        recs[0].LateMinutes.Should().Be(0);
    }
}
```
*(Note: fix the helper name typo — use a single consistent `Effctx` method name in both the declaration and call sites.)*

- [ ] **Step 3: Run to verify fail**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter AttendanceExcuseExecutorTests`
Expected: FAIL — correction leaves minutes stale; leave inserts a duplicate row.

- [ ] **Step 4: Fix `AttendanceCorrectionExecutor`**

In both branches where `Status = AttendanceStatus.Present` is set, also zero the penalty minutes. New record branch: add `LateMinutes = 0, ShortageMinutes = 0` to the initializer. Existing branch: after `existing.Status = AttendanceStatus.Present;` add:
```csharp
            existing.LateMinutes = 0;
            existing.ShortageMinutes = 0;
```

- [ ] **Step 5: Fix `AttendanceApplyLeaveDaysExecutor` (upsert)**

Replace the blind `_db.AttendanceRecords.Add(...)` loop body with an upsert. The method must become `async` (it currently returns `Task.FromResult`). New body:
```csharp
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
```
Change the method signature from `public Task<EffectExecutionResult> ExecuteAsync(...)` to `public async Task<EffectExecutionResult> ExecuteAsync(...)` and drop the `Task.FromResult(...)` wrapper.

- [ ] **Step 6: Run tests + full suite**

Run: `cd backend && dotnet test tests/HR.Domain.Finance.Tests --filter AttendanceExcuseExecutorTests`
Then: `dotnet test tests/HR.Domain.Finance.Tests`
Expected: PASS. (If adding the `HR.Modules.Attendance` reference surfaces any DI/type clash in the test project, resolve the `using`s; the executors only need `ApplicationDbContext` + `EffectContext`.)

- [ ] **Step 7: Commit**

```bash
git add backend/src/HR.Modules/Attendance/Completion/AttendanceCorrectionExecutor.cs \
        backend/src/HR.Modules/Attendance/Completion/AttendanceApplyLeaveDaysExecutor.cs \
        backend/tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj \
        backend/tests/HR.Domain.Finance.Tests/AttendanceExcuseExecutorTests.cs
git commit -m "fix(payroll-2e): excuse/leave executors zero penalty minutes so cancel-on-zero fires"
```

---

## Task 5: Permission + daily attendance payroll-impact endpoint

**Files:**
- Modify: `backend/src/HR.Infrastructure/Persistence/SeedData.cs`
- Create migration: `backend/src/HR.Infrastructure/Migrations/<ts>_AttendancePayrollImpactPermission.cs`
- Modify: `backend/src/HR.Modules/Attendance/Controllers/AttendanceController.cs`

**Interfaces:**
- Consumes: `IAttendancePayrollSyncService.SyncAsync(version, period, [employeeId], includeOvertime, ct)`.
- Produces: `POST /api/attendance/payroll-impact/sync` (perm `Attendance.PayrollImpact.Create`) → `AttendancePayrollSyncReport`.

- [ ] **Step 1: Seed the new permission**

In `backend/src/HR.Infrastructure/Persistence/SeedData.cs`, add to the `modules` dictionary (after the `["Attendance"]` line):
```csharp
            ["Attendance.PayrollImpact"] = new[] { "Create" },
```
This produces permission code `Attendance.PayrollImpact.Create` via the existing `{Module}.{Name}` seeding, with a deterministic Id. The Super Admin template (`AllPermissions`) will include it when templates are (re)seeded.

- [ ] **Step 2: Generate the seed-data migration**

Run: `cd backend/src/HR.Infrastructure && dotnet ef migrations add AttendancePayrollImpactPermission --startup-project ../HR.Api`
Expected: a migration whose `Up()` is a single `InsertData` into `Permissions` (the one new row). Open it and confirm it inserts only the new permission — no other data/schema changes.

- [ ] **Step 3: Add the endpoint + DTO to `AttendanceController`**

Inject the sync service and add the endpoint. Update the ctor:
```csharp
    private readonly IAttendanceService _svc;
    private readonly HR.Application.Engines.Finance.IAttendancePayrollSyncService _payrollSync;
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _db;
    public AttendanceController(IAttendanceService svc,
        HR.Application.Engines.Finance.IAttendancePayrollSyncService payrollSync,
        HR.Infrastructure.Persistence.ApplicationDbContext db)
    { _svc = svc; _payrollSync = payrollSync; _db = db; }
```

Add the request DTO (top of the controller file namespace, or reuse the controller's nested-class style):
```csharp
public sealed class SyncAttendancePayrollImpactRequest
{
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public bool IncludeOvertime { get; set; }
}
```

Add the endpoint:
```csharp
    [HttpPost("payroll-impact/sync")]
    [RequirePermission("Attendance.PayrollImpact.Create")]
    public async Task<ActionResult<ApiResponse<HR.Application.Engines.Finance.AttendancePayrollSyncReport>>> SyncPayrollImpact(
        [FromBody] SyncAttendancePayrollImpactRequest req, CancellationToken ct)
    {
        var version = await _db.PayrollDefinitionVersions.AsNoTracking()
            .Where(v => v.Status == HR.Domain.Enums.VersionStatus.Published)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync(ct)
            ?? throw new HR.Application.Common.Exceptions.DomainException("No published payroll version is available.");

        var report = await _payrollSync.SyncAsync(
            version, HR.Domain.Engines.Finance.PayrollPeriod.Monthly(req.Year, req.Month),
            new[] { req.EmployeeId }, includeOvertime: req.IncludeOvertime, ct: ct);
        return OkResponse(report, $"Synced {report.TotalProcessed} attendance line(s) for the employee.");
    }
```
Add the needed `using`s (`Microsoft.EntityFrameworkCore` for the query; `HR.Api.Filters` for `RequirePermission` is already imported). Verify `PayrollPeriod.Monthly` and `VersionStatus.Published` are the correct references (the payroll controller uses `PayrollPeriod.Monthly` and definitions publish a `Published` version).

- [ ] **Step 4: Build**

Run: `cd backend && dotnet build`
Expected: 0 errors. (If `OkResponse(value, message)` two-arg form isn't available, drop the message — confirm against `PayrollController`'s usage.)

- [ ] **Step 5: Commit**

```bash
git add backend/src/HR.Infrastructure/Persistence/SeedData.cs \
        backend/src/HR.Infrastructure/Migrations \
        backend/src/HR.Modules/Attendance/Controllers/AttendanceController.cs
git commit -m "feat(payroll-2e): Attendance.PayrollImpact.Create permission + daily payroll-impact sync endpoint"
```

---

## Task 6: Frontend — daily attendance payroll-impact actions

**Files:**
- Modify: `src/lib/api/attendance.ts`
- Modify: `src/app/(dashboard)/attendance/page.tsx` (`DailyTable`)

**Interfaces:**
- Consumes: `POST /api/attendance/payroll-impact/sync`.

- [ ] **Step 1: Add the api client function** — in `src/lib/api/attendance.ts`, mirroring existing calls (`addManualPunch`/`correctAttendance` use the project's api helper):

```ts
export interface AttendancePayrollSyncReport {
  created: number; updated: number; removed: number; skippedPosted: number; totalProcessed: number;
}
export async function syncAttendancePayrollImpact(body: {
  employeeId: string; year: number; month: number; includeOvertime: boolean;
}): Promise<AttendancePayrollSyncReport> {
  const res = await apiPost("/api/attendance/payroll-impact/sync", body); // use the same helper addManualPunch uses
  return res.data;
}
```
Match the exact helper/return-unwrapping style already used in this file (some clients return `res.data`, some return the response directly — copy the sibling functions).

- [ ] **Step 2: Add per-row actions to `DailyTable`**

In `src/app/(dashboard)/attendance/page.tsx`:
- Compute a permission flag near the existing `canEdit`: `const canPayrollImpact = hasAny("Attendance.PayrollImpact.Create");`
- Add a target-month selector to the daily view toolbar (default to the currently-viewed month/year).
- In each `DailyTable` row's action `<td>` (where view/punch/correct buttons live), when `canPayrollImpact`, add a small action menu with four items: *Calculate absence deduction*, *Calculate late deduction*, *Calculate shortage deduction*, *Calculate overtime addition*. Each calls `syncAttendancePayrollImpact({ employeeId: row.employeeId, year, month, includeOvertime })` where `includeOvertime` is `true` only for the overtime action (the three deduction actions pass `false`; they run the deduction sync which is unaffected by the overtime flag).
- On success show a toast summarising the report (`Created ${r.created}, Updated ${r.updated}, Removed ${r.removed}, Skipped ${r.skippedPosted}`).
- The four deduction/overtime actions all hit the same endpoint for that employee+month; they differ only in the `includeOvertime` flag. (Per the design, the daily row is the entry point; the materialised records are per-employee/period aggregates surfaced on `/payroll/deductions` and `/payroll/additions`.)

- [ ] **Step 3: Build the frontend**

Run: `npm run build` (repo root)
Expected: build succeeds; tsc clean. Fix any type errors (mirror sibling api client signatures).

- [ ] **Step 4: Commit**

```bash
git add src/
git commit -m "feat(payroll-2e): daily attendance payroll-impact actions (deductions + overtime)"
```

---

## Task 7: Full verification

**Files:** none (verification only).

- [ ] **Step 1: Backend build + full test run**

Run: `cd backend && dotnet build && dotnet test`
Expected: build 0 errors; all test projects pass (finance suite includes the new 2E tests + the renamed 2D tests).

- [ ] **Step 2: Confirm only the permission seed migration is pending model-wise**

Run: `cd backend/src/HR.Infrastructure && dotnet ef migrations has-pending-model-changes --startup-project ../HR.Api`
Expected: no pending model changes (the permission `HasData` change is captured by the Task 5 migration; nothing else changed the model).

- [ ] **Step 3: Frontend build**

Run: `npm run build`
Expected: succeeds.

- [ ] **Step 4: Deploy checklist (do NOT deploy without user authorization)**

Mirrors 2D:
1. `dotnet ef database update` against Azure Postgres (applies the permission `InsertData`).
2. Re-seed access templates so Super Admin picks up `Attendance.PayrollImpact.Create` (via the app's template-seed path); verify the admin can call the endpoint.
3. Publish + zip-deploy the API (forward-slash entries) to `hrcloud-api-v4xd`.
4. Vercel auto-deploys the frontend.
5. Live-verify: an employee with overtime → daily "Calculate overtime addition" → an OVERTIME Addition appears on `/payroll/additions` at 1.5×; approve an excuse for a late day → the late deduction is Cancelled on the next sync.

- [ ] **Step 5: Final commit (if any verification fixes were needed)**

```bash
git add -A
git commit -m "chore(payroll-2e): verification pass — full build + tests green"
```

---

## Self-Review Notes (coverage against the spec)

- Spec §4.1 rename + Overtime kind → **T1** (rename) + **T3** (wire Overtime). §4.2 configurable rates + IncludeOvertime → **T2**; overtime multiplier applied → **T3**. §4.3 wage basis (÷8) unchanged (reuses fact `HourlyWage`). §4.4 daily endpoint + FE → **T5** (endpoint) + **T6** (FE). §4.5 excuse/leave fix → **T4**. §4.6 audit origin → StatusReason on the created transactions (set in T3's `StatusReason` on cancel; origin marker on create can reuse the existing audit — minor, covered by the sync's existing provenance). §5/§6 error handling → DomainException in T3 (`ResolveTypesAsync`) + T5 (no version). §7 tests → distributed T2/T3/T4 + build gates T5/T6. §8 one permission migration → **T5**. §9 acceptance → verified in **T7**.
- Type consistency: `AttendancePayrollKind`, `AttendancePayrollSyncService`, `IAttendancePayrollSyncService`, `AttendancePayrollSyncReport`, `PayrollCalcSettings.Rates`/`.IncludeOvertime`/`AttendanceRates`, `SyncAsync(..., bool? includeOvertime = null, CancellationToken ct = default)` are consistent across tasks.
- Open verification points flagged inline: the api-helper unwrapping style in `attendance.ts` (T6 S1), `OkResponse` two-arg overload (T5 S3), `VersionStatus.Published`/`PayrollPeriod.Monthly` references (T5 S3), and the test-helper method-name typo to fix (T4 S2).

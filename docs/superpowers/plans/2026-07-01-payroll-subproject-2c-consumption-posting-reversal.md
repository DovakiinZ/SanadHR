# Payroll 2C — Transaction Consumption, Posting & Reversal — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make approved addition/deduction records flow into payroll runs as per-record payslip lines, post each to the immutable ledger on execution, and allow correcting posted records via reversal + next-period correction — without reopening closed runs.

**Architecture:** Three touch-points on the existing run lifecycle (Calculate → consume, Execute → post, anytime → reverse), reusing the append-only `FinancialLedgerEntry` ledger and the `PayrollTransactionStateMachine` (`Approved → Posted → Reversed`) unchanged. Consumed transactions become `ComponentResult` lines (code `TXN:{id:N}`) merged into each employee's `RuleSetEvaluation`; the ledger mapper tags their postings with `ReferenceType="PayrollTransaction"`/`ReferenceId=txnId`; the executor stamps posting metadata back onto the transaction. Reversal nets the money via the ledger's existing counter-entry.

**Tech Stack:** .NET 8, EF Core 8 (Npgsql), xUnit + FluentAssertions, Next.js (App Router) + TypeScript frontend.

## Global Constraints

- Spec: `docs/superpowers/specs/2026-07-01-payroll-subproject-2c-consumption-posting-reversal-design.md`. Every task implicitly inherits it.
- **No new migration** — all columns exist (`PayrollTransaction.PayrollRunId/PostedAt/PostedBy/LedgerEntryId/ReversesTransactionId/ReversalReason`; ledger `ReferenceType/ReferenceId`).
- **Business-rule errors throw `HR.Application.Common.Exceptions.DomainException`** (maps to HTTP 422 via the Track-1 hotfix middleware). Never `InvalidOperationException` for new rules.
- **Postgres `timestamptz` requires UTC `DateTime`** — normalize any persisted date with `DateTime.SpecifyKind(d, DateTimeKind.Utc)` (InMemory tests do NOT catch this; only real Npgsql does).
- `PayrollTransaction` and `EmployeeAdditions/EmployeeDeductions` are **disjoint** — do NOT touch the seeded `ADDITIONS`/`DEDUCTIONS` rules or `PayrollFactProvider` (no double-count).
- Tests live in `backend/tests/HR.Domain.Finance.Tests` (added to `HR.sln`). Reuse the `FakeUser : ICurrentUserService` + in-memory `ApplicationDbContext` harness pattern from `PayrollTransactionServiceTests.cs`.
- Build from `backend/`: `dotnet build src/HR.Api/HR.Api.csproj -c Debug`. Test: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj`.
- Permissions: reversal endpoint gated `Payroll.Approve`; impact preview `Payroll.View`. No new permission keys.

---

### Task 1: `PayrollPeriodResolver` — cutoff-aware period resolution (pure)

**Files:**
- Create: `backend/src/HR.Domain/Engines/Finance/PayrollPeriodResolver.cs`
- Test: `backend/tests/HR.Domain.Finance.Tests/PayrollPeriodResolverTests.cs`

**Interfaces:**
- Produces: `static (int Year, int Month) PayrollPeriodResolver.Resolve(DateTime effectiveDate, int cutoffDay, bool carryToNextPeriod)`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using HR.Domain.Engines.Finance;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollPeriodResolverTests
{
    [Fact]
    public void Before_cutoff_stays_in_same_month() =>
        PayrollPeriodResolver.Resolve(new DateTime(2026, 7, 20), 27, true).Should().Be((2026, 7));

    [Fact]
    public void On_cutoff_day_stays_in_same_month() =>
        PayrollPeriodResolver.Resolve(new DateTime(2026, 7, 27), 27, true).Should().Be((2026, 7));

    [Fact]
    public void After_cutoff_with_carry_moves_to_next_month() =>
        PayrollPeriodResolver.Resolve(new DateTime(2026, 7, 28), 27, true).Should().Be((2026, 8));

    [Fact]
    public void After_cutoff_without_carry_stays_in_same_month() =>
        PayrollPeriodResolver.Resolve(new DateTime(2026, 7, 28), 27, false).Should().Be((2026, 7));

    [Fact]
    public void December_after_cutoff_rolls_into_next_year() =>
        PayrollPeriodResolver.Resolve(new DateTime(2026, 12, 31), 27, true).Should().Be((2027, 1));
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PayrollPeriodResolverTests`
Expected: FAIL — `PayrollPeriodResolver` does not exist (compile error).

- [ ] **Step 3: Write minimal implementation**

```csharp
namespace HR.Domain.Engines.Finance;

/// <summary>Resolves which payroll period a dated transaction belongs to, honoring the definition
/// version's cutoff. A transaction whose EffectiveDate falls after the cutoff day carries to the next
/// period when the version allows it. Pure — no DB, no I/O — reused by the run-time consumer and the
/// create-time impact preview so the two never drift.</summary>
public static class PayrollPeriodResolver
{
    public static (int Year, int Month) Resolve(DateTime effectiveDate, int cutoffDay, bool carryToNextPeriod)
    {
        var year = effectiveDate.Year;
        var month = effectiveDate.Month;
        if (carryToNextPeriod && effectiveDate.Day > cutoffDay)
        {
            month++;
            if (month > 12) { month = 1; year++; }
        }
        return (year, month);
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PayrollPeriodResolverTests`
Expected: PASS (5 tests).

- [ ] **Step 5: Commit**

```bash
git add backend/src/HR.Domain/Engines/Finance/PayrollPeriodResolver.cs backend/tests/HR.Domain.Finance.Tests/PayrollPeriodResolverTests.cs
git commit -m "feat(payroll-2c): cutoff-aware PayrollPeriodResolver"
```

---

### Task 2: `IPayrollTransactionConsumer` — load consumable transactions

**Files:**
- Create: `backend/src/HR.Application/Engines/Finance/IPayrollTransactionConsumer.cs`
- Create: `backend/src/HR.Infrastructure/Engines/Finance/PayrollTransactionConsumer.cs`
- Modify: `backend/src/HR.Infrastructure/DependencyInjection.cs` (register the service near the other Finance registrations, ~line 67)
- Test: `backend/tests/HR.Domain.Finance.Tests/PayrollTransactionConsumerTests.cs`

**Interfaces:**
- Consumes: `PayrollPeriodResolver.Resolve` (Task 1)
- Produces:
  - `record ConsumableTransaction(Guid TransactionId, Guid EmployeeId, PayrollTransactionKind Kind, string TypeCode, decimal Amount, DateTime EffectiveDate)`
  - `IPayrollTransactionConsumer.GetConsumableAsync(int periodYear, int periodMonth, IReadOnlyCollection<Guid> employeeIds, int cutoffDay, bool carryToNextPeriod, CancellationToken) → Task<IReadOnlyList<ConsumableTransaction>>`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.MasterData;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTransactionConsumerTests
{
    private static readonly Guid Tenant = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Tenant;
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.View" };
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    private static DateTime Utc(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_only_approved_transactions_resolved_into_the_period()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var emp = new Employee { EmployeeNumber = "E1", FirstName = "Ali", LastName = "S", Email = "a@t.local" };
        db.Employees.Add(emp);
        var addType = new MasterDataItem { ObjectType = MasterDataObjectType.AdditionType, Code = "BONUS", NameAr = "م", NameEn = "Bonus" };
        db.MasterDataItems.Add(addType);
        await db.SaveChangesAsync();

        // in-period, approved -> consumed
        db.PayrollTransactions.Add(new PayrollTransaction { Kind = PayrollTransactionKind.Addition, EmployeeId = emp.Id, TypeId = addType.Id, Amount = 500m, EffectiveDate = Utc(2026, 7, 10), TransactionDate = Utc(2026, 7, 10), Status = PayrollTransactionStatus.Approved });
        // in-period but Draft -> excluded
        db.PayrollTransactions.Add(new PayrollTransaction { Kind = PayrollTransactionKind.Addition, EmployeeId = emp.Id, TypeId = addType.Id, Amount = 999m, EffectiveDate = Utc(2026, 7, 11), TransactionDate = Utc(2026, 7, 11), Status = PayrollTransactionStatus.Draft });
        // after cutoff -> carried to August -> excluded from July
        db.PayrollTransactions.Add(new PayrollTransaction { Kind = PayrollTransactionKind.Addition, EmployeeId = emp.Id, TypeId = addType.Id, Amount = 700m, EffectiveDate = Utc(2026, 7, 29), TransactionDate = Utc(2026, 7, 29), Status = PayrollTransactionStatus.Approved });
        await db.SaveChangesAsync();

        var sut = new PayrollTransactionConsumer(db);
        var result = await sut.GetConsumableAsync(2026, 7, new[] { emp.Id }, cutoffDay: 27, carryToNextPeriod: true);

        result.Should().HaveCount(1);
        result[0].Amount.Should().Be(500m);
        result[0].TypeCode.Should().Be("BONUS");
        result[0].Kind.Should().Be(PayrollTransactionKind.Addition);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PayrollTransactionConsumerTests`
Expected: FAIL — `IPayrollTransactionConsumer`/`PayrollTransactionConsumer`/`ConsumableTransaction` do not exist.

- [ ] **Step 3: Write the interface**

`backend/src/HR.Application/Engines/Finance/IPayrollTransactionConsumer.cs`:
```csharp
using HR.Domain.Enums;

namespace HR.Application.Engines.Finance;

/// <summary>One approved payroll transaction resolved as eligible for a specific run period.</summary>
public sealed record ConsumableTransaction(
    Guid TransactionId,
    Guid EmployeeId,
    PayrollTransactionKind Kind,
    string TypeCode,
    decimal Amount,
    DateTime EffectiveDate);

/// <summary>Loads the approved addition/deduction records a run should consume for its period, applying
/// cutoff carry-over. Read-only — never writes.</summary>
public interface IPayrollTransactionConsumer
{
    Task<IReadOnlyList<ConsumableTransaction>> GetConsumableAsync(
        int periodYear, int periodMonth,
        IReadOnlyCollection<Guid> employeeIds,
        int cutoffDay, bool carryToNextPeriod,
        CancellationToken ct = default);
}
```

- [ ] **Step 4: Write the implementation**

`backend/src/HR.Infrastructure/Engines/Finance/PayrollTransactionConsumer.cs`:
```csharp
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

public sealed class PayrollTransactionConsumer : IPayrollTransactionConsumer
{
    private readonly ApplicationDbContext _db;

    public PayrollTransactionConsumer(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<ConsumableTransaction>> GetConsumableAsync(
        int periodYear, int periodMonth,
        IReadOnlyCollection<Guid> employeeIds,
        int cutoffDay, bool carryToNextPeriod,
        CancellationToken ct = default)
    {
        if (employeeIds.Count == 0) return Array.Empty<ConsumableTransaction>();

        // All approved transactions for the population; period membership is resolved in memory via the
        // shared cutoff rule so the consumer and the impact preview can never drift.
        var approved = await _db.PayrollTransactions.AsNoTracking()
            .Where(t => t.Status == PayrollTransactionStatus.Approved && employeeIds.Contains(t.EmployeeId))
            .Select(t => new { t.Id, t.EmployeeId, t.Kind, t.TypeId, t.Amount, t.EffectiveDate })
            .ToListAsync(ct);

        var inPeriod = approved
            .Where(t => PayrollPeriodResolver.Resolve(t.EffectiveDate, cutoffDay, carryToNextPeriod) == (periodYear, periodMonth))
            .ToList();
        if (inPeriod.Count == 0) return Array.Empty<ConsumableTransaction>();

        var typeIds = inPeriod.Select(t => t.TypeId).Distinct().ToList();
        var typeCodes = await _db.MasterDataItems.AsNoTracking()
            .Where(m => typeIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.Code, ct);

        return inPeriod
            .Select(t => new ConsumableTransaction(
                t.Id, t.EmployeeId, t.Kind,
                typeCodes.TryGetValue(t.TypeId, out var code) ? code : "TXN",
                t.Amount, t.EffectiveDate))
            .ToList();
    }
}
```

- [ ] **Step 5: Register in DI**

In `backend/src/HR.Infrastructure/DependencyInjection.cs`, directly after the `IPayrollTransactionService` registration (~line 67), add:
```csharp
        services.AddScoped<HR.Application.Engines.Finance.IPayrollTransactionConsumer, HR.Infrastructure.Engines.Finance.PayrollTransactionConsumer>();
```

- [ ] **Step 6: Run test to verify it passes**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PayrollTransactionConsumerTests`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add backend/src/HR.Application/Engines/Finance/IPayrollTransactionConsumer.cs backend/src/HR.Infrastructure/Engines/Finance/PayrollTransactionConsumer.cs backend/src/HR.Infrastructure/DependencyInjection.cs backend/tests/HR.Domain.Finance.Tests/PayrollTransactionConsumerTests.cs
git commit -m "feat(payroll-2c): PayrollTransactionConsumer (approved+in-period, cutoff-aware) + DI"
```

---

### Task 3: `PayrollTransactionMerge` + wire into `PayrollComputation`

**Files:**
- Create: `backend/src/HR.Application/Engines/Finance/PayrollTransactionMerge.cs`
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/PayrollComputation.cs` (ctor + `ComputeAsync`)
- Test: `backend/tests/HR.Domain.Finance.Tests/PayrollTransactionMergeTests.cs`

**Interfaces:**
- Consumes: `ConsumableTransaction` (Task 2), `IPayrollTransactionConsumer` (Task 2), `RuleSetEvaluation`/`ComponentResult` (`HR.Domain.Engines.Finance.RuleEngineCore`)
- Produces: `static RuleSetEvaluation PayrollTransactionMerge.Apply(RuleSetEvaluation evaluation, IReadOnlyList<ConsumableTransaction> txns)` and `const string PayrollTransactionMerge.ComponentCodePrefix = "TXN:"`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Enums;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTransactionMergeTests
{
    [Fact]
    public void Apply_adds_addition_as_earning_and_deduction_and_recomputes_totals()
    {
        var baseEval = new RuleSetEvaluation(
            new List<ComponentResult> { new("BASIC", "BASIC", PayComponentKind.Earning, 1000m, true) },
            new List<string> { "BASIC" }, 1000m, 0m, 1000m);

        var txns = new List<ConsumableTransaction>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), PayrollTransactionKind.Addition, "BONUS", 200m, default),
            new(Guid.NewGuid(), Guid.NewGuid(), PayrollTransactionKind.Deduction, "PENALTY", 50m, default),
        };

        var result = PayrollTransactionMerge.Apply(baseEval, txns);

        result.Components.Should().HaveCount(3);
        result.GrossEarnings.Should().Be(1200m);
        result.TotalDeductions.Should().Be(50m);
        result.NetAmount.Should().Be(1150m);
        result.Components[1].ComponentCode.Should().Be("BONUS");
        result.Components[1].Code.Should().StartWith("TXN:");
        result.Components[1].Kind.Should().Be(PayComponentKind.Earning);
        result.Components[2].Kind.Should().Be(PayComponentKind.Deduction);
    }

    [Fact]
    public void Apply_with_no_transactions_returns_evaluation_unchanged()
    {
        var baseEval = new RuleSetEvaluation(
            new List<ComponentResult> { new("BASIC", "BASIC", PayComponentKind.Earning, 1000m, true) },
            new List<string> { "BASIC" }, 1000m, 0m, 1000m);

        PayrollTransactionMerge.Apply(baseEval, new List<ConsumableTransaction>()).Should().BeSameAs(baseEval);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PayrollTransactionMergeTests`
Expected: FAIL — `PayrollTransactionMerge` does not exist.

- [ ] **Step 3: Write the merge helper**

`backend/src/HR.Application/Engines/Finance/PayrollTransactionMerge.cs`:
```csharp
using HR.Domain.Engines.Finance;
using HR.Domain.Enums;

namespace HR.Application.Engines.Finance;

/// <summary>Folds consumed payroll transactions into a rule-set evaluation as per-record components,
/// recomputing gross/deductions/net. Pure — unit-testable without a DB or rule engine.</summary>
public static class PayrollTransactionMerge
{
    /// <summary>Stable component-code prefix identifying a transaction-sourced line. The ledger mapper
    /// parses the transaction id from the suffix to tag the posting.</summary>
    public const string ComponentCodePrefix = "TXN:";

    public static RuleSetEvaluation Apply(RuleSetEvaluation evaluation, IReadOnlyList<ConsumableTransaction> txns)
    {
        if (txns.Count == 0) return evaluation;

        var components = new List<ComponentResult>(evaluation.Components);
        var order = new List<string>(evaluation.ExecutionOrder);
        var gross = evaluation.GrossEarnings;
        var deductions = evaluation.TotalDeductions;

        foreach (var t in txns)
        {
            var kind = t.Kind == PayrollTransactionKind.Addition ? PayComponentKind.Earning : PayComponentKind.Deduction;
            var code = $"{ComponentCodePrefix}{t.TransactionId:N}";
            components.Add(new ComponentResult(code, t.TypeCode, kind, t.Amount, true));
            order.Add(code);
            if (kind == PayComponentKind.Earning) gross += t.Amount; else deductions += t.Amount;
        }

        return new RuleSetEvaluation(
            components, order,
            Math.Round(gross, 2, MidpointRounding.AwayFromZero),
            Math.Round(deductions, 2, MidpointRounding.AwayFromZero),
            Math.Round(gross - deductions, 2, MidpointRounding.AwayFromZero));
    }
}
```

- [ ] **Step 4: Run merge test to verify it passes**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PayrollTransactionMergeTests`
Expected: PASS (2 tests).

- [ ] **Step 5: Wire the merge into `PayrollComputation`**

In `backend/src/HR.Infrastructure/Engines/Finance/PayrollComputation.cs`:

(a) Add the consumer to the constructor. Replace the field/ctor block (lines 21–30) with:
```csharp
    private readonly ApplicationDbContext _db;
    private readonly IPayrollFactProvider _facts;
    private readonly IRuleEngine _rules;
    private readonly IPayrollTransactionConsumer _consumer;

    public PayrollComputation(ApplicationDbContext db, IPayrollFactProvider facts, IRuleEngine rules,
        IPayrollTransactionConsumer consumer)
    {
        _db = db;
        _facts = facts;
        _rules = rules;
        _consumer = consumer;
    }
```

(b) At the end of `ComputeAsync`, immediately BEFORE `return new PayrollComputationResult(...)` (currently line 77), insert:
```csharp
        // 2C: consume approved addition/deduction records as per-record components (read-only, used by
        // both preview and calculate). PayrollTransaction is disjoint from EmployeeAdditions/Deductions,
        // so these are additive — no double count.
        if (results.Count > 0)
        {
            var empIds = results.Select(r => r.EmployeeId).ToList();
            var consumables = await _consumer.GetConsumableAsync(
                period.Year, period.Month, empIds, version.CutoffDay, version.CarryToNextPeriod, ct);
            if (consumables.Count > 0)
            {
                var byEmp = consumables.GroupBy(c => c.EmployeeId).ToDictionary(g => g.Key, g => (IReadOnlyList<ConsumableTransaction>)g.ToList());
                for (var i = 0; i < results.Count; i++)
                {
                    if (byEmp.TryGetValue(results[i].EmployeeId, out var txns))
                        results[i] = results[i] with { Evaluation = PayrollTransactionMerge.Apply(results[i].Evaluation, txns) };
                }
            }
        }
```
Note: `PayrollComputation.cs` already has `using HR.Application.Engines.Finance;` and `using HR.Domain.Enums;` is not present — add `using HR.Domain.Enums;` is NOT needed here (no enum literals used). `PayrollTransactionMerge` and `ConsumableTransaction` are in `HR.Application.Engines.Finance` (already imported). `period.Year`/`period.Month` are real properties on `PayrollPeriod`. `version.CutoffDay`/`version.CarryToNextPeriod` are on `PayrollDefinitionVersion`.

- [ ] **Step 6: Build to verify the wiring compiles**

Run: `dotnet build src/HR.Api/HR.Api.csproj -c Debug`
Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Run full finance test suite (no regressions)**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj`
Expected: PASS (all prior tests + the new ones).

- [ ] **Step 8: Commit**

```bash
git add backend/src/HR.Application/Engines/Finance/PayrollTransactionMerge.cs backend/src/HR.Infrastructure/Engines/Finance/PayrollComputation.cs backend/tests/HR.Domain.Finance.Tests/PayrollTransactionMergeTests.cs
git commit -m "feat(payroll-2c): merge consumed transactions into computation as per-record components"
```

---

### Task 4: Tag transaction postings in `PayslipLedgerMapper`

**Files:**
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/PayslipLedgerMapper.cs`
- Test: `backend/tests/HR.Domain.Finance.Tests/PayslipLedgerMapperTests.cs` (add a test to the existing class)

**Interfaces:**
- Consumes: `PayrollTransactionMerge.ComponentCodePrefix` (Task 3)
- Produces: `const string PayslipLedgerMapper.TransactionReference = "PayrollTransaction"`; postings for `TXN:`-prefixed components carry `ReferenceType="PayrollTransaction"`, `ReferenceId=<txnId>`.

- [ ] **Step 1: Write the failing test (append to `PayslipLedgerMapperTests`)**

```csharp
    [Fact]
    public void Transaction_sourced_component_is_tagged_with_its_transaction_reference()
    {
        var txnId = Guid.NewGuid();
        var payslip = Payslip(
            ("BASIC", PayComponentKind.Earning, 1000m, true),
            ($"TXN:{txnId:N}", PayComponentKind.Earning, 200m, true));

        var postings = PayslipLedgerMapper.Map(Guid.NewGuid(), payslip);

        var txnPosting = postings.Single(p => p.ReferenceType == "PayrollTransaction");
        txnPosting.ReferenceId.Should().Be(txnId);
        txnPosting.Amount.Should().Be(200m);
        postings.Should().Contain(p => p.ReferenceType == "PayrollPayslip" && p.Amount == 1000m);
    }
```
(The existing `Payslip(...)` helper sets both `Code` and `ComponentCode` to the supplied code, so the `TXN:{guid}` value lands in `Code` and the parser picks it up.)

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PayslipLedgerMapperTests`
Expected: FAIL — no posting has `ReferenceType == "PayrollTransaction"` (currently all are `"PayrollPayslip"`).

- [ ] **Step 3: Implement the tagging**

In `PayslipLedgerMapper.cs`:

(a) Add the constant under the existing `PayslipReference` const (line 14):
```csharp
    public const string TransactionReference = "PayrollTransaction";
```

(b) Add `using HR.Application.Engines.Finance;` at the top (for `PayrollTransactionMerge.ComponentCodePrefix`).

(c) Inside the `foreach (var c in comps.EnumerateArray())` loop, replace the existing `postings.Add(new LedgerPostingRequest { ... });` block with: read the `Code`, build the base posting, then override the reference for transaction-sourced lines:
```csharp
            var code = c.TryGetProperty("Code", out var cd) ? cd.GetString() : null;

            var referenceType = PayslipReference;
            Guid referenceId = payslip.Id;
            if (code is not null
                && code.StartsWith(PayrollTransactionMerge.ComponentCodePrefix, StringComparison.Ordinal)
                && Guid.TryParseExact(code[PayrollTransactionMerge.ComponentCodePrefix.Length..], "N", out var txnId))
            {
                referenceType = TransactionReference;
                referenceId = txnId;
            }

            postings.Add(new LedgerPostingRequest
            {
                EmployeeId = payslip.EmployeeId,
                SourceModule = FinanceSourceModule.Payroll,
                ComponentCode = componentCode,
                Amount = Math.Abs(amount),
                Currency = payslip.Currency,
                Direction = direction,
                Description = $"Payroll {componentCode} for {payslip.EmployeeNumber}",
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                PayrollRunId = runId,
                EntryNumber = $"PRL-{payslip.Id:N}-{index:D2}",
            });
            index++;
```
(Leave the lines that compute `applied`, `amount`, `kind`, `direction`, `componentCode` above this block unchanged.)

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PayslipLedgerMapperTests`
Expected: PASS (existing tests + the new one).

- [ ] **Step 5: Commit**

```bash
git add backend/src/HR.Infrastructure/Engines/Finance/PayslipLedgerMapper.cs backend/tests/HR.Domain.Finance.Tests/PayslipLedgerMapperTests.cs
git commit -m "feat(payroll-2c): tag per-transaction ledger postings with PayrollTransaction reference"
```

---

### Task 5: `PostedTransactionStamper` + wire into `PayrollItemExecutor`

**Files:**
- Create: `backend/src/HR.Infrastructure/Engines/Finance/PostedTransactionStamper.cs`
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/PayrollItemExecutor.cs` (one call inside `ExecuteItemAsync`'s `try`)
- Test: `backend/tests/HR.Domain.Finance.Tests/PostedTransactionStamperTests.cs`

**Interfaces:**
- Consumes: `PayslipLedgerMapper.TransactionReference` (Task 4)
- Produces: `static Task PostedTransactionStamper.StampAsync(ApplicationDbContext db, Guid runId, Guid employeeId, CancellationToken ct)` — flips Approved transactions referenced by this run+employee's `PayrollTransaction` ledger entries to `Posted` with `PayrollRunId`/`PostedAt`/`LedgerEntryId`. Idempotent.

- [ ] **Step 1: Write the failing test**

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

public class PostedTransactionStamperTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.Lock" };
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    [Fact]
    public async Task Stamps_referenced_transaction_as_posted_and_leaves_others_untouched()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var runId = Guid.NewGuid();
        var empId = Guid.NewGuid();
        var postedAt = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

        var consumed = new PayrollTransaction { Kind = PayrollTransactionKind.Addition, EmployeeId = empId, TypeId = Guid.NewGuid(), Amount = 200m, EffectiveDate = postedAt, TransactionDate = postedAt, Status = PayrollTransactionStatus.Approved };
        var untouched = new PayrollTransaction { Kind = PayrollTransactionKind.Addition, EmployeeId = empId, TypeId = Guid.NewGuid(), Amount = 9m, EffectiveDate = postedAt, TransactionDate = postedAt, Status = PayrollTransactionStatus.Approved };
        db.PayrollTransactions.AddRange(consumed, untouched);
        await db.SaveChangesAsync();

        var entry = new FinancialLedgerEntry { EntryNumber = "PRL-x-00", EmployeeId = empId, ComponentCode = "BONUS", Amount = 200m, Direction = LedgerDirection.Credit, PayrollRunId = runId, ReferenceType = "PayrollTransaction", ReferenceId = consumed.Id, PostedAt = postedAt };
        db.FinancialLedgerEntries.Add(entry);
        await db.SaveChangesAsync();

        await PostedTransactionStamper.StampAsync(db, runId, empId, default);

        var reloadedConsumed = await db.PayrollTransactions.SingleAsync(t => t.Id == consumed.Id);
        reloadedConsumed.Status.Should().Be(PayrollTransactionStatus.Posted);
        reloadedConsumed.PayrollRunId.Should().Be(runId);
        reloadedConsumed.LedgerEntryId.Should().Be(entry.Id);
        reloadedConsumed.PostedAt.Should().Be(postedAt);

        var reloadedUntouched = await db.PayrollTransactions.SingleAsync(t => t.Id == untouched.Id);
        reloadedUntouched.Status.Should().Be(PayrollTransactionStatus.Approved);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PostedTransactionStamperTests`
Expected: FAIL — `PostedTransactionStamper` does not exist.

- [ ] **Step 3: Write the stamper**

`backend/src/HR.Infrastructure/Engines/Finance/PostedTransactionStamper.cs`:
```csharp
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Stamps posting metadata onto payroll transactions once their per-record ledger entries exist.
/// Idempotent: only flips still-Approved transactions, so re-running a failed execution converges and a
/// transaction is never double-stamped.</summary>
public static class PostedTransactionStamper
{
    public static async Task StampAsync(ApplicationDbContext db, Guid runId, Guid employeeId, CancellationToken ct)
    {
        var entries = await db.FinancialLedgerEntries
            .Where(e => e.PayrollRunId == runId && e.EmployeeId == employeeId
                        && e.ReferenceType == PayslipLedgerMapper.TransactionReference && e.ReferenceId != null)
            .Select(e => new { e.Id, TxnId = e.ReferenceId!.Value, e.PostedAt })
            .ToListAsync(ct);
        if (entries.Count == 0) return;

        var txnIds = entries.Select(e => e.TxnId).ToList();
        var txns = await db.PayrollTransactions
            .Where(t => txnIds.Contains(t.Id) && t.Status == PayrollTransactionStatus.Approved)
            .ToListAsync(ct);

        foreach (var t in txns)
        {
            var entry = entries.First(e => e.TxnId == t.Id);
            t.Status = PayrollTransactionStatus.Posted;
            t.PayrollRunId = runId;
            t.PostedAt = entry.PostedAt;
            t.LedgerEntryId = entry.Id;
        }
        // Saved by the caller (PayrollItemExecutor) inside the same unit of work.
    }
}
```

- [ ] **Step 4: Run stamper test to verify it passes**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PostedTransactionStamperTests`
Expected: PASS.

- [ ] **Step 5: Wire into `PayrollItemExecutor.ExecuteItemAsync`**

In `backend/src/HR.Infrastructure/Engines/Finance/PayrollItemExecutor.cs`, inside the `try`, AFTER the `payslip.LedgerPosted = true; payslip.LedgerPostedAt = DateTime.UtcNow;` lines and BEFORE `item.State = PayrollRunItemState.Completed;`, add:
```csharp
            // 2C: flip consumed transactions to Posted (idempotent; runs on fresh + resumed executions).
            await PostedTransactionStamper.StampAsync(_db, item.PayrollRunId, payslip.EmployeeId, ct);
```
This is saved by the existing `await _db.SaveChangesAsync(ct);` that follows.

- [ ] **Step 6: Build + full suite**

Run: `dotnet build src/HR.Api/HR.Api.csproj -c Debug` → 0 errors.
Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj` → all PASS.

- [ ] **Step 7: Commit**

```bash
git add backend/src/HR.Infrastructure/Engines/Finance/PostedTransactionStamper.cs backend/src/HR.Infrastructure/Engines/Finance/PayrollItemExecutor.cs backend/tests/HR.Domain.Finance.Tests/PostedTransactionStamperTests.cs
git commit -m "feat(payroll-2c): stamp consumed transactions Posted on execution (idempotent)"
```

---

### Task 6: `PayrollTransactionReversalService` + DI

**Files:**
- Create: `backend/src/HR.Application/Engines/Finance/IPayrollTransactionReversalService.cs`
- Create: `backend/src/HR.Infrastructure/Engines/Finance/PayrollTransactionReversalService.cs`
- Modify: `backend/src/HR.Infrastructure/DependencyInjection.cs` (register near other Finance services)
- Test: `backend/tests/HR.Domain.Finance.Tests/PayrollTransactionReversalServiceTests.cs`

**Interfaces:**
- Consumes: `IFinancialLedger.ReverseAsync` (`HR.Application.Engines.Finance`), `PayrollTransactionStateMachine` (`HR.Domain.Engines.Finance.StateMachine`), `DomainException`/`NotFoundException` (`HR.Application.Common.Exceptions`)
- Produces:
  - `record ReversalResult(Guid ReversedTransactionId, Guid CounterLedgerEntryId, Guid? CorrectionTransactionId)`
  - `IPayrollTransactionReversalService.ReverseAsync(Guid transactionId, string reason, bool createCorrection, decimal? correctedAmount, CancellationToken) → Task<ReversalResult>`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTransactionReversalServiceTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.Approve" };
        public bool IsAuthenticated => true;
    }

    private sealed class FakeLedger : IFinancialLedger
    {
        public List<Guid> Reversed { get; } = new();
        public Task<FinancialLedgerEntry> ReverseAsync(Guid entryId, string reason, CancellationToken ct = default)
        {
            Reversed.Add(entryId);
            return Task.FromResult(new FinancialLedgerEntry { Id = Guid.NewGuid(), EntryNumber = "REV-1", Amount = 0m });
        }
        public Task<FinancialLedgerEntry> PostAsync(LedgerPostingRequest r, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<FinancialLedgerEntry>> PostManyAsync(IEnumerable<LedgerPostingRequest> r, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<decimal> GetEmployeeBalanceAsync(Guid e, string c = "SAR", CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<FinancialLedgerEntry>> QueryAsync(LedgerQuery q, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    private static PayrollTransaction PostedTxn(Guid ledgerEntryId) => new()
    {
        Kind = PayrollTransactionKind.Deduction, EmployeeId = Guid.NewGuid(), TypeId = Guid.NewGuid(),
        Amount = 100m, EffectiveDate = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc),
        TransactionDate = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc),
        Status = PayrollTransactionStatus.Posted, LedgerEntryId = ledgerEntryId,
    };

    [Fact]
    public async Task Reverse_posted_transaction_marks_reversed_and_reverses_ledger_entry()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var ledgerEntryId = Guid.NewGuid();
        var txn = PostedTxn(ledgerEntryId);
        db.PayrollTransactions.Add(txn);
        await db.SaveChangesAsync();

        var ledger = new FakeLedger();
        var sut = new PayrollTransactionReversalService(db, ledger, new FakeUser());

        var result = await sut.ReverseAsync(txn.Id, "duplicate entry", createCorrection: false, correctedAmount: null, default);

        var reloaded = await db.PayrollTransactions.SingleAsync(t => t.Id == txn.Id);
        reloaded.Status.Should().Be(PayrollTransactionStatus.Reversed);
        reloaded.ReversalReason.Should().Be("duplicate entry");
        ledger.Reversed.Should().ContainSingle().Which.Should().Be(ledgerEntryId);
        result.CorrectionTransactionId.Should().BeNull();
    }

    [Fact]
    public async Task Reverse_with_correction_creates_a_draft_correction_in_a_new_period()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var txn = PostedTxn(Guid.NewGuid());
        db.PayrollTransactions.Add(txn);
        await db.SaveChangesAsync();

        var sut = new PayrollTransactionReversalService(db, new FakeLedger(), new FakeUser());

        var result = await sut.ReverseAsync(txn.Id, "wrong amount", createCorrection: true, correctedAmount: 80m, default);

        result.CorrectionTransactionId.Should().NotBeNull();
        var correction = await db.PayrollTransactions.SingleAsync(t => t.Id == result.CorrectionTransactionId);
        correction.Status.Should().Be(PayrollTransactionStatus.Draft);
        correction.Amount.Should().Be(80m);
        correction.SourceModule.Should().Be("Correction");
        correction.ReversesTransactionId.Should().Be(txn.Id);
        correction.Kind.Should().Be(txn.Kind);
        correction.EffectiveDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Reversing_a_non_posted_transaction_throws_DomainException()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var txn = PostedTxn(Guid.NewGuid());
        txn.Status = PayrollTransactionStatus.Draft;
        db.PayrollTransactions.Add(txn);
        await db.SaveChangesAsync();

        var sut = new PayrollTransactionReversalService(db, new FakeLedger(), new FakeUser());

        var act = async () => await sut.ReverseAsync(txn.Id, "x", false, null, default);
        await act.Should().ThrowAsync<DomainException>();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PayrollTransactionReversalServiceTests`
Expected: FAIL — service/interface do not exist.

- [ ] **Step 3: Write the interface**

`backend/src/HR.Application/Engines/Finance/IPayrollTransactionReversalService.cs`:
```csharp
namespace HR.Application.Engines.Finance;

public sealed record ReversalResult(Guid ReversedTransactionId, Guid CounterLedgerEntryId, Guid? CorrectionTransactionId);

/// <summary>Reverses a posted payroll transaction (counter ledger entry + Posted→Reversed) and optionally
/// creates a Draft correction that flows into the next run. Posted records are never edited in place.</summary>
public interface IPayrollTransactionReversalService
{
    Task<ReversalResult> ReverseAsync(Guid transactionId, string reason, bool createCorrection,
        decimal? correctedAmount, CancellationToken ct = default);
}
```

- [ ] **Step 4: Write the implementation**

`backend/src/HR.Infrastructure/Engines/Finance/PayrollTransactionReversalService.cs`:
```csharp
using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.Finance.StateMachine;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

public sealed class PayrollTransactionReversalService : IPayrollTransactionReversalService
{
    private readonly ApplicationDbContext _db;
    private readonly IFinancialLedger _ledger;
    private readonly ICurrentUserService _user;

    public PayrollTransactionReversalService(ApplicationDbContext db, IFinancialLedger ledger, ICurrentUserService user)
    {
        _db = db;
        _ledger = ledger;
        _user = user;
    }

    private static DateTime AsUtc(DateTime d) => d.Kind == DateTimeKind.Utc ? d : DateTime.SpecifyKind(d, DateTimeKind.Utc);

    public async Task<ReversalResult> ReverseAsync(Guid transactionId, string reason, bool createCorrection,
        decimal? correctedAmount, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A reversal reason is required.");

        var txn = await _db.PayrollTransactions.FirstOrDefaultAsync(t => t.Id == transactionId, ct)
            ?? throw new NotFoundException("PayrollTransaction", transactionId);

        if (txn.Status != PayrollTransactionStatus.Posted)
            throw new DomainException("Only a posted transaction can be reversed.");
        if (txn.LedgerEntryId is not { } ledgerEntryId)
            throw new DomainException("This transaction has no ledger entry to reverse.");

        var counter = await _ledger.ReverseAsync(ledgerEntryId, reason, ct);

        PayrollTransactionStateMachine.EnsureCanTransition(txn.Status, PayrollTransactionStatus.Reversed);
        txn.Status = PayrollTransactionStatus.Reversed;
        txn.ReversalReason = reason;

        Guid? correctionId = null;
        if (createCorrection)
        {
            if (correctedAmount is not { } amount || amount < 0)
                throw new DomainException("A non-negative corrected amount is required to create a correction.");

            var today = AsUtc(DateTime.UtcNow.Date);
            var correction = new PayrollTransaction
            {
                Kind = txn.Kind,
                EmployeeId = txn.EmployeeId,
                TypeId = txn.TypeId,
                Amount = amount,
                EffectiveDate = today,
                TransactionDate = today,
                TargetPeriodYear = today.Year,
                TargetPeriodMonth = today.Month,
                Notes = $"Correction of reversed transaction {txn.Id}",
                SourceModule = "Correction",
                ReversesTransactionId = txn.Id,
                Status = PayrollTransactionStatus.Draft,
            };
            _db.PayrollTransactions.Add(correction);
            await _db.SaveChangesAsync(ct);
            correctionId = correction.Id;
        }
        else
        {
            await _db.SaveChangesAsync(ct);
        }

        return new ReversalResult(txn.Id, counter.Id, correctionId);
    }
}
```

- [ ] **Step 5: Register in DI**

In `backend/src/HR.Infrastructure/DependencyInjection.cs`, after the `IPayrollTransactionConsumer` registration (Task 2), add:
```csharp
        services.AddScoped<HR.Application.Engines.Finance.IPayrollTransactionReversalService, HR.Infrastructure.Engines.Finance.PayrollTransactionReversalService>();
```

- [ ] **Step 6: Run test to verify it passes**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj --filter PayrollTransactionReversalServiceTests`
Expected: PASS (3 tests).

- [ ] **Step 7: Commit**

```bash
git add backend/src/HR.Application/Engines/Finance/IPayrollTransactionReversalService.cs backend/src/HR.Infrastructure/Engines/Finance/PayrollTransactionReversalService.cs backend/src/HR.Infrastructure/DependencyInjection.cs backend/tests/HR.Domain.Finance.Tests/PayrollTransactionReversalServiceTests.cs
git commit -m "feat(payroll-2c): PayrollTransactionReversalService (reverse + next-period correction) + DI"
```

---

### Task 7: Reverse + impact-preview endpoints on `PayrollController`

**Files:**
- Modify: `backend/src/HR.Modules/Payroll/Controllers/PayrollController.cs` (ctor + 2 endpoints)
- Modify: `backend/src/HR.Modules/Payroll/Controllers/PayrollDtos.cs` (2 request/response DTOs — confirm filename; it is the file holding `RejectTransactionRequest`)

**Interfaces:**
- Consumes: `IPayrollTransactionReversalService` (Task 6), `PayrollPeriodResolver` (Task 1), existing `IPayrollTransactionService.GetAsync`
- Produces: `POST /api/payroll/transactions/{id}/reverse`, `GET /api/payroll/transactions/impact-preview`

- [ ] **Step 1: Add the DTOs**

In the DTOs file that defines `RejectTransactionRequest`, add:
```csharp
public sealed record ReverseTransactionRequest(string Reason, bool CreateCorrection, decimal? CorrectedAmount);

public sealed record TransactionImpactDto(int PeriodYear, int PeriodMonth, int CutoffDay, bool CarriedAfterCutoff);
```

- [ ] **Step 2: Inject the reversal service into the controller**

In `PayrollController.cs`, add a field + ctor parameter (mirroring `_transactions`):
- Add field: `private readonly IPayrollTransactionReversalService _reversals;`
- Add ctor parameter `IPayrollTransactionReversalService reversals` and `_reversals = reversals;`
- Ensure `using HR.Application.Engines.Finance;` is present (it is — `IPayrollTransactionService` is used).

- [ ] **Step 3: Add the reverse endpoint**

Add next to the other `transactions/{id:guid}/...` actions:
```csharp
    [HttpPost("transactions/{id:guid}/reverse")]
    [RequirePermission("Payroll.Approve")]
    public async Task<ActionResult<ApiResponse<PayrollTransactionDto>>> ReverseTransaction(
        Guid id, [FromBody] ReverseTransactionRequest req, CancellationToken ct)
    {
        await _reversals.ReverseAsync(id, req.Reason, req.CreateCorrection, req.CorrectedAmount, ct);
        return OkResponse(await _transactions.GetAsync(id, ct));
    }
```

- [ ] **Step 4: Add the impact-preview endpoint**

```csharp
    [HttpGet("transactions/impact-preview")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<TransactionImpactDto>>> TransactionImpactPreview(
        [FromQuery] DateTime effectiveDate, CancellationToken ct)
    {
        // Use the most recent payroll definition version's cutoff (the standard MONTHLY cycle in 2C).
        var version = await _db.PayrollDefinitionVersions.AsNoTracking()
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new { v.CutoffDay, v.CarryToNextPeriod })
            .FirstOrDefaultAsync(ct);
        var cutoffDay = version?.CutoffDay ?? 27;
        var carry = version?.CarryToNextPeriod ?? true;
        var (year, month) = PayrollPeriodResolver.Resolve(effectiveDate, cutoffDay, carry);
        var carried = carry && effectiveDate.Day > cutoffDay;
        return OkResponse(new TransactionImpactDto(year, month, cutoffDay, carried));
    }
```
Add `using HR.Domain.Engines.Finance;` to the controller for `PayrollPeriodResolver` (confirm not already imported).

- [ ] **Step 5: Build to verify the controller compiles**

Run: `dotnet build src/HR.Api/HR.Api.csproj -c Debug`
Expected: Build succeeded, 0 errors. (`AuditableEntity`/`TenantEntity` expose `CreatedAt` for the `OrderByDescending`; if `PayrollDefinitionVersion` lacks `CreatedAt`, order by `v.VersionNumber` instead — verify the property name in `PayrollDefinition.cs` and adjust.)

- [ ] **Step 6: Commit**

```bash
git add backend/src/HR.Modules/Payroll/Controllers/PayrollController.cs backend/src/HR.Modules/Payroll/Controllers/PayrollDtos.cs
git commit -m "feat(payroll-2c): reverse + impact-preview endpoints on PayrollController"
```

---

### Task 8: Frontend — reverse action, impact preview, per-record run lines

**Files:**
- Modify: `src/lib/api/payroll-transactions.ts` (add `reverseTransaction`, `getTransactionImpact`)
- Modify: `src/components/payroll/transactions-page.tsx` (Reverse action on Posted rows; impact preview in create form)
- Verify/modify: `src/app/(dashboard)/payroll/runs/[id]/page.tsx` (or the payslip-components renderer) so `TXN:`-prefixed components display as their `ComponentCode` label

**Interfaces:**
- Consumes: `POST /api/payroll/transactions/{id}/reverse`, `GET /api/payroll/transactions/impact-preview` (Task 7)

> The frontend has no automated test harness; each step ends with a manual verification against the live API.

- [ ] **Step 1: Add API wrappers**

In `src/lib/api/payroll-transactions.ts`, add (mirroring the existing `submitTransaction`/`approveTransaction` wrappers and their `apiFetch` usage):
```ts
export function reverseTransaction(
  id: string,
  body: { reason: string; createCorrection: boolean; correctedAmount?: number },
) {
  return apiPost(`/api/payroll/transactions/${id}/reverse`, body);
}

export function getTransactionImpact(effectiveDate: string) {
  return apiGet<{ periodYear: number; periodMonth: number; cutoffDay: number; carriedAfterCutoff: boolean }>(
    `/api/payroll/transactions/impact-preview?effectiveDate=${encodeURIComponent(effectiveDate)}`,
  );
}
```
(Match the exact helper names already imported in this file — e.g. if the file uses `api.post`/`api.get`, use those instead of `apiPost`/`apiGet`.)

- [ ] **Step 2: Manual verify the wrappers compile + call**

Run: `npm run build` (from repo root) → no type errors in `payroll-transactions.ts`.

- [ ] **Step 3: Add the Reverse action to Posted rows**

In `src/components/payroll/transactions-page.tsx`:
- Gate a "Reverse" button on rows whose `status` is `Posted`, behind `usePermissions().has("Payroll.Approve")`.
- On click, open a dialog capturing `reason` (required) and an optional "create correction" checkbox + `correctedAmount` number input.
- Submit → `reverseTransaction(row.id, { reason, createCorrection, correctedAmount })`; on success `toast.success` + reload the list. Reuse the existing list-reload function. Errors surface via the existing `notifyError` (422/409 messages now show — see the Track-1 hotfix).

- [ ] **Step 4: Add the impact preview to the create form**

In the create/edit transaction form within `transactions-page.tsx`, when `effectiveDate` changes, call `getTransactionImpact(effectiveDate)` and render a hint, e.g.:
`سيُحتسب في رواتب {periodMonth}/{periodYear}` + (when `carriedAfterCutoff`) `— بعد تاريخ الإقفال (يوم {cutoffDay})`.

- [ ] **Step 5: Ensure run-details shows per-record lines**

Open `src/app/(dashboard)/payroll/runs/[id]/page.tsx` (and the component that renders a payslip's `components`). Confirm each component row renders using its `ComponentCode` (label) and `Amount`. Transaction lines already arrive in `ComponentsJson` (code `TXN:...`, componentCode = the type code like `BONUS`), so they appear automatically. If the renderer hides unknown codes, add a fallback that shows the `componentCode` for any code starting with `TXN:`.

- [ ] **Step 6: Manual end-to-end verification (against the deployed API after Task 9)**

1. Create an addition for an employee, submit + approve it.
2. Create a payroll run for that period, Calculate → confirm the addition appears as its own line on the employee's payslip.
3. Execute the run → confirm the transaction status flips to `Posted` (GET `/api/payroll/transactions?...`) and a ledger entry with `ReferenceType=PayrollTransaction` exists.
4. Reverse the posted transaction with a correction → confirm status `Reversed` and a new `Draft` correction appears.

- [ ] **Step 7: Commit**

```bash
git add src/lib/api/payroll-transactions.ts src/components/payroll/transactions-page.tsx "src/app/(dashboard)/payroll/runs/[id]/page.tsx"
git commit -m "feat(payroll-2c): FE reverse action, impact preview, per-record run lines"
```

---

### Task 9: Full build, test, and deploy

**Files:** none (verification + deploy)

- [ ] **Step 1: Full backend build**

Run: `dotnet build src/HR.Api/HR.Api.csproj -c Debug`
Expected: Build succeeded, 0 errors.

- [ ] **Step 2: Full finance test suite**

Run: `dotnet test tests/HR.Domain.Finance.Tests/HR.Domain.Finance.Tests.csproj`
Expected: all PASS (prior 141 + the new ~14).

- [ ] **Step 3: Frontend build**

Run (repo root): `npm run build`
Expected: compiles with no type errors.

- [ ] **Step 4: Confirm no migration is needed**

Run: `dotnet ef migrations has-pending-model-changes --project src/HR.Infrastructure/HR.Infrastructure.csproj --startup-project src/HR.Api/HR.Api.csproj` (or `dotnet ef migrations add Probe2C --no-build` and confirm it generates an EMPTY `Up`/`Down`, then delete it).
Expected: NO model changes (the 2C code adds no entity/column).

- [ ] **Step 5: Commit any final fixes, then deploy (with user approval)**

Publish + zip (forward-slash entries) + `az webapp deploy --resource-group HR --name hrcloud-api-v4xd --type zip`; push the branch to `sanad`. Verify live with the end-to-end steps from Task 8 Step 6.

```bash
git add -A && git commit -m "chore(payroll-2c): final build/test fixes"
git push sanad feat/financial-engine
```

---

## Self-Review

- **Spec coverage:** Consumption (Tasks 2–3), per-record components (Task 3), posting + metadata (Tasks 4–5), reversal + next-period correction (Task 6), cutoff carry-over (Task 1, used in 2 & 3), impact preview (Tasks 7–8), run-details per-record display (Task 8), DomainException error handling (Tasks 6–7), tests (each task), no migration (Task 9 Step 4), no double-count (Global Constraints; no fact/rule changes). All spec sections map to a task.
- **Placeholders:** none — every code step shows complete code; the two "confirm the property/helper name" notes (FE helper names in Task 8 Step 1; `CreatedAt` vs `VersionNumber` in Task 7 Step 5) are explicit verify-and-adjust instructions with the fallback named, not gaps.
- **Type consistency:** `ConsumableTransaction` (Task 2) is consumed unchanged in Tasks 3; `PayrollTransactionMerge.ComponentCodePrefix = "TXN:"` (Task 3) is the exact prefix parsed in Task 4 and rendered in Task 8; `PayslipLedgerMapper.TransactionReference = "PayrollTransaction"` (Task 4) is the exact string queried in Task 5; `IPayrollTransactionReversalService.ReverseAsync(...)` signature (Task 6) matches the controller call (Task 7).

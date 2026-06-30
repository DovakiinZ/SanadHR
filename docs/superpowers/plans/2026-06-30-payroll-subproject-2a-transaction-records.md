# Payroll Sub-project 2A — Transaction Records, Lifecycle & Pages — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a unified, lifecycle-governed `PayrollTransaction` record (additions & deductions) with full CRUD/approval API and two HR/Finance pages, so every addition/deduction is a visible, traceable record before payroll — with no engine consumption yet.

**Architecture:** One domain entity `PayrollTransaction : TenantEntity` with a `Kind` discriminator, governed by a `PayrollTransactionStateMachine` modeled on the existing `PayrollRunStateMachine`. Writes go through a new `IPayrollTransactionService` (concrete `ApplicationDbContext`, no MediatR — matching the Payroll module). The existing `PayrollController` (`api/payroll`) gains transaction endpoints gated by existing `Payroll.*` permissions. Frontend adds a typed api wrapper plus a shared page component rendered by `/payroll/additions` and `/payroll/deductions`.

**Tech Stack:** .NET 8, EF Core 8 (Npgsql/PostgreSQL), xUnit 2.9.2 + FluentAssertions 6.12.1 + EF Core InMemory 8.0.10; Next.js App Router (RTL/Arabic), shadcn/ui + sonner, TypeScript.

## Global Constraints

- New entities inherit `HR.Domain.Common.TenantEntity` — **never** set `TenantId`/`CreatedAt`/`CreatedBy`/`UpdatedAt` manually; `ApplicationDbContext.SaveChangesAsync` sets them automatically. Tenant reads are globally filtered.
- Backend tables use snake_case `engine_payroll_*` naming; money is `decimal(18,2)`; enums persist as **int** by default (no string converter).
- Payroll module uses **direct service + `ApplicationDbContext` injection, not MediatR**. Controllers gate every method with `[RequirePermission("Payroll.X")]` (namespace `HR.Api.Filters`, OR-semantics) plus class-level `[Authorize]`, and return `OkResponse(...)`/`CreatedResponse(...)` from `BaseApiController`.
- Valid payroll permission codes (from `SeedData.cs`): `Payroll.View`, `Payroll.Create`, `Payroll.Edit`, `Payroll.Delete`, `Payroll.Approve`, `Payroll.Export`, `Payroll.Run`, `Payroll.Lock`, `Payroll.Configure`. **Reuse these — do not invent new codes.**
- `IApplicationDbContext` has no DbSets; add the new DbSet only to the concrete `ApplicationDbContext`.
- Frontend pages never touch the token or call `fetch` directly — they call typed wrappers in `src/lib/api/*.ts` which use `apiFetch<T>`. There is **no shadcn `Select`**; use native `<select>` or `@/components/ui/combobox`.
- AGENTS.md: this is a modified Next.js — read the relevant guide in `node_modules/next/dist/docs/` before writing Next.js code.
- Local `appsettings.json` stays on `localhost` — never commit prod secrets.
- Run backend tests with: `dotnet test backend/tests/HR.Domain.Finance.Tests --filter <ClassName>` (single class) or `dotnet test backend/HR.sln` (all).

---

### Task 1: Lifecycle enums + state machine (domain)

**Files:**
- Create: `backend/src/HR.Domain/Enums/PayrollTransactionEnums.cs`
- Create: `backend/src/HR.Domain/Engines/Finance/StateMachine/PayrollTransactionStateMachine.cs`
- Test: `backend/tests/HR.Domain.Finance.Tests/PayrollTransactionStateMachineTests.cs`

**Interfaces:**
- Produces: `enum PayrollTransactionKind { Addition = 1, Deduction = 2 }`; `enum PayrollTransactionStatus { Draft = 0, PendingApproval = 1, Approved = 2, Rejected = 3, Cancelled = 4, CarriedForward = 5, Posted = 6, Reversed = 7 }` (namespace `HR.Domain.Enums`). Static class `PayrollTransactionStateMachine` with `NextStates`, `CanTransition`, `EnsureCanTransition`, `IsImmutable`, `IsTerminal`; `sealed class InvalidPayrollTransactionStateException : Exception` carrying `From`/`To` (namespace `HR.Domain.Engines.Finance.StateMachine`).

- [ ] **Step 1: Write the enums**

Create `backend/src/HR.Domain/Enums/PayrollTransactionEnums.cs`:

```csharp
namespace HR.Domain.Enums;

/// <summary>Whether a payroll transaction adds to or subtracts from net pay. Sign is implied by Kind;
/// the stored Amount is always non-negative.</summary>
public enum PayrollTransactionKind
{
    Addition = 1,
    Deduction = 2,
}

/// <summary>The full lifecycle of an addition/deduction record. Draft→PendingApproval→Approved are wired in
/// sub-project 2A; Posted/CarriedForward/Reversed are reached by the payroll engine in 2B.</summary>
public enum PayrollTransactionStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    CarriedForward = 5,
    Posted = 6,
    Reversed = 7,
}
```

- [ ] **Step 2: Write the failing state-machine test**

Create `backend/tests/HR.Domain.Finance.Tests/PayrollTransactionStateMachineTests.cs`:

```csharp
using FluentAssertions;
using HR.Domain.Engines.Finance.StateMachine;
using HR.Domain.Enums;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTransactionStateMachineTests
{
    [Theory]
    [InlineData(PayrollTransactionStatus.Draft, PayrollTransactionStatus.PendingApproval)]
    [InlineData(PayrollTransactionStatus.Draft, PayrollTransactionStatus.Cancelled)]
    [InlineData(PayrollTransactionStatus.PendingApproval, PayrollTransactionStatus.Approved)]
    [InlineData(PayrollTransactionStatus.PendingApproval, PayrollTransactionStatus.Rejected)]
    [InlineData(PayrollTransactionStatus.Rejected, PayrollTransactionStatus.Draft)]
    [InlineData(PayrollTransactionStatus.Approved, PayrollTransactionStatus.Cancelled)]
    [InlineData(PayrollTransactionStatus.Approved, PayrollTransactionStatus.Posted)]
    [InlineData(PayrollTransactionStatus.Approved, PayrollTransactionStatus.CarriedForward)]
    [InlineData(PayrollTransactionStatus.CarriedForward, PayrollTransactionStatus.Posted)]
    [InlineData(PayrollTransactionStatus.Posted, PayrollTransactionStatus.Reversed)]
    public void Allows_legal_transitions(PayrollTransactionStatus from, PayrollTransactionStatus to)
    {
        PayrollTransactionStateMachine.CanTransition(from, to).Should().BeTrue();
    }

    [Theory]
    [InlineData(PayrollTransactionStatus.Draft, PayrollTransactionStatus.Approved)]   // can't skip approval
    [InlineData(PayrollTransactionStatus.Draft, PayrollTransactionStatus.Posted)]     // can't skip to posted
    [InlineData(PayrollTransactionStatus.Approved, PayrollTransactionStatus.Draft)]   // approved is locked
    [InlineData(PayrollTransactionStatus.Posted, PayrollTransactionStatus.Approved)]  // posted is immutable
    [InlineData(PayrollTransactionStatus.Cancelled, PayrollTransactionStatus.Draft)]  // terminal
    [InlineData(PayrollTransactionStatus.Reversed, PayrollTransactionStatus.Posted)]  // terminal
    public void Rejects_illegal_transitions(PayrollTransactionStatus from, PayrollTransactionStatus to)
    {
        PayrollTransactionStateMachine.CanTransition(from, to).Should().BeFalse();
    }

    [Fact]
    public void EnsureCanTransition_throws_on_illegal_move()
    {
        var act = () => PayrollTransactionStateMachine.EnsureCanTransition(
            PayrollTransactionStatus.Draft, PayrollTransactionStatus.Posted);
        act.Should().Throw<InvalidPayrollTransactionStateException>();
    }

    [Fact]
    public void Posted_is_immutable()
    {
        PayrollTransactionStateMachine.IsImmutable(PayrollTransactionStatus.Posted).Should().BeTrue();
        PayrollTransactionStateMachine.IsImmutable(PayrollTransactionStatus.Draft).Should().BeFalse();
        PayrollTransactionStateMachine.IsImmutable(PayrollTransactionStatus.Approved).Should().BeFalse();
    }

    [Fact]
    public void Terminal_states_have_no_successors()
    {
        PayrollTransactionStateMachine.NextStates(PayrollTransactionStatus.Cancelled).Should().BeEmpty();
        PayrollTransactionStateMachine.NextStates(PayrollTransactionStatus.Reversed).Should().BeEmpty();
        PayrollTransactionStateMachine.IsTerminal(PayrollTransactionStatus.Cancelled).Should().BeTrue();
        PayrollTransactionStateMachine.IsTerminal(PayrollTransactionStatus.Reversed).Should().BeTrue();
    }
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test backend/tests/HR.Domain.Finance.Tests --filter PayrollTransactionStateMachineTests`
Expected: FAIL — compile error / `PayrollTransactionStateMachine` does not exist.

- [ ] **Step 4: Write the state machine**

Create `backend/src/HR.Domain/Engines/Finance/StateMachine/PayrollTransactionStateMachine.cs`:

```csharp
using HR.Domain.Enums;

namespace HR.Domain.Engines.Finance.StateMachine;

/// <summary>Raised when an illegal payroll-transaction lifecycle transition is attempted.</summary>
public sealed class InvalidPayrollTransactionStateException : Exception
{
    public PayrollTransactionStatus From { get; }
    public PayrollTransactionStatus To { get; }

    public InvalidPayrollTransactionStateException(PayrollTransactionStatus from, PayrollTransactionStatus to)
        : base($"Illegal payroll transaction transition: {from} → {to}.")
    {
        From = from;
        To = to;
    }
}

/// <summary>The single source of truth for the addition/deduction lifecycle. Draft→PendingApproval→Approved
/// (and Rejected/Cancelled) are driven by HR/Finance in sub-project 2A; Posted/CarriedForward/Reversed are
/// driven by the payroll engine in 2B. Cancelled and Reversed are terminal.</summary>
public static class PayrollTransactionStateMachine
{
    private static readonly IReadOnlyDictionary<PayrollTransactionStatus, PayrollTransactionStatus[]> Allowed =
        new Dictionary<PayrollTransactionStatus, PayrollTransactionStatus[]>
        {
            [PayrollTransactionStatus.Draft] = new[] { PayrollTransactionStatus.PendingApproval, PayrollTransactionStatus.Cancelled },
            [PayrollTransactionStatus.PendingApproval] = new[] { PayrollTransactionStatus.Approved, PayrollTransactionStatus.Rejected },
            [PayrollTransactionStatus.Rejected] = new[] { PayrollTransactionStatus.Draft },
            [PayrollTransactionStatus.Approved] = new[] { PayrollTransactionStatus.Cancelled, PayrollTransactionStatus.Posted, PayrollTransactionStatus.CarriedForward },
            [PayrollTransactionStatus.CarriedForward] = new[] { PayrollTransactionStatus.Posted, PayrollTransactionStatus.Cancelled },
            [PayrollTransactionStatus.Posted] = new[] { PayrollTransactionStatus.Reversed },
            [PayrollTransactionStatus.Cancelled] = Array.Empty<PayrollTransactionStatus>(),
            [PayrollTransactionStatus.Reversed] = Array.Empty<PayrollTransactionStatus>(),
        };

    public static IReadOnlyList<PayrollTransactionStatus> NextStates(PayrollTransactionStatus from) =>
        Allowed.TryGetValue(from, out var next) ? next : Array.Empty<PayrollTransactionStatus>();

    public static bool CanTransition(PayrollTransactionStatus from, PayrollTransactionStatus to) =>
        Allowed.TryGetValue(from, out var next) && Array.IndexOf(next, to) >= 0;

    /// <summary>Throws <see cref="InvalidPayrollTransactionStateException"/> if the transition is not permitted.</summary>
    public static void EnsureCanTransition(PayrollTransactionStatus from, PayrollTransactionStatus to)
    {
        if (!CanTransition(from, to)) throw new InvalidPayrollTransactionStateException(from, to);
    }

    /// <summary>True once the transaction is financially frozen (Posted). Posted records are corrected only
    /// via reversal (2B).</summary>
    public static bool IsImmutable(PayrollTransactionStatus state) => state is PayrollTransactionStatus.Posted;

    public static bool IsTerminal(PayrollTransactionStatus state) =>
        state is PayrollTransactionStatus.Cancelled or PayrollTransactionStatus.Reversed;

    /// <summary>True only in Draft — the sole state in which a transaction may be edited.</summary>
    public static bool IsEditable(PayrollTransactionStatus state) => state is PayrollTransactionStatus.Draft;
}
```

- [ ] **Step 5: Run the test to verify it passes**

Run: `dotnet test backend/tests/HR.Domain.Finance.Tests --filter PayrollTransactionStateMachineTests`
Expected: PASS (all theories + facts).

- [ ] **Step 6: Commit**

```bash
git add backend/src/HR.Domain/Enums/PayrollTransactionEnums.cs backend/src/HR.Domain/Engines/Finance/StateMachine/PayrollTransactionStateMachine.cs backend/tests/HR.Domain.Finance.Tests/PayrollTransactionStateMachineTests.cs
git commit -m "feat(payroll): PayrollTransaction lifecycle enums + state machine (2A)"
```

---

### Task 2: `PayrollTransaction` entity + EF config + DbSet + migration

**Files:**
- Create: `backend/src/HR.Domain/Engines/Finance/Entities/PayrollTransaction.cs`
- Modify: `backend/src/HR.Infrastructure/Persistence/Configurations/Engines/FinanceConfigurations.cs` (add a config class at end of file)
- Modify: `backend/src/HR.Infrastructure/Persistence/ApplicationDbContext.cs` (add DbSet next to the other finance DbSets, ~line 259)
- Test: `backend/tests/HR.Domain.Finance.Tests/PayrollTransactionPersistenceTests.cs`

**Interfaces:**
- Consumes: `PayrollTransactionKind`, `PayrollTransactionStatus` (Task 1).
- Produces: `class PayrollTransaction : TenantEntity` (namespace `HR.Domain.Engines.Finance.Entities`); `ApplicationDbContext.PayrollTransactions` DbSet.

- [ ] **Step 1: Write the entity**

Create `backend/src/HR.Domain/Engines/Finance/Entities/PayrollTransaction.cs`:

```csharp
using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Finance.Entities;

/// <summary>A single dated payroll addition or deduction. Distinct from the recurring per-employee
/// EmployeeAddition/EmployeeDeduction components: this is a traceable, approvable, period-bound record that
/// exists before payroll runs. Sub-project 2A manages it up to Approved; the engine consumes it in 2B.</summary>
public class PayrollTransaction : TenantEntity
{
    /// <summary>Addition or deduction. Sign is implied by Kind; <see cref="Amount"/> is always non-negative.</summary>
    public PayrollTransactionKind Kind { get; set; }

    public Guid EmployeeId { get; set; }

    /// <summary>References a MasterDataItem whose ObjectType is "AdditionType" (Kind=Addition) or
    /// "DeductionType" (Kind=Deduction).</summary>
    public Guid TypeId { get; set; }

    /// <summary>Non-negative amount in the tenant currency.</summary>
    public decimal Amount { get; set; }

    /// <summary>When the business event occurred (e.g. the day the bonus/penalty was decided).</summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>The date that drives payroll-period selection and (in 2B) cutoff. All business calculation
    /// uses this date.</summary>
    public DateTime EffectiveDate { get; set; }

    /// <summary>Intended payroll period (year), derived from EffectiveDate on create. Display-only in 2A.</summary>
    public int? TargetPeriodYear { get; set; }

    /// <summary>Intended payroll period (month 1-12), derived from EffectiveDate on create.</summary>
    public int? TargetPeriodMonth { get; set; }

    /// <summary>Flag only in 2A; per-period materialization is implemented in 2B.</summary>
    public bool IsRecurring { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }

    public string? Notes { get; set; }

    /// <summary>Optional StoredFile id, served via /api/files/{id}.</summary>
    public Guid? AttachmentFileId { get; set; }

    /// <summary>Provenance: "Manual" in 2A; "Attendance"/"Loan"/... set by source modules in 2B/2C.</summary>
    public string SourceModule { get; set; } = "Manual";

    /// <summary>Traceability back to an originating record (e.g. "AttendanceRecord").</summary>
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }

    public PayrollTransactionStatus Status { get; set; } = PayrollTransactionStatus.Draft;

    /// <summary>Reason captured on reject/cancel/reversal.</summary>
    public string? StatusReason { get; set; }

    // --- Posting metadata: columns defined in 2A, populated by the engine in 2B. ---
    public Guid? PayrollRunId { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public Guid? LedgerEntryId { get; set; }

    // --- Reversal link: defined in 2A, transition wired in 2B. ---
    public Guid? ReversesTransactionId { get; set; }
    public string? ReversalReason { get; set; }
}
```

- [ ] **Step 2: Add the EF configuration**

Append this class inside `backend/src/HR.Infrastructure/Persistence/Configurations/Engines/FinanceConfigurations.cs` (same namespace/usings as the existing configs in that file; it already imports `HR.Domain.Engines.Finance.Entities`, `Microsoft.EntityFrameworkCore`, and `Microsoft.EntityFrameworkCore.Metadata.Builders`):

```csharp
public class PayrollTransactionConfiguration : IEntityTypeConfiguration<PayrollTransaction>
{
    public void Configure(EntityTypeBuilder<PayrollTransaction> builder)
    {
        builder.ToTable("engine_payroll_transactions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.SourceModule).HasMaxLength(60).IsRequired();
        builder.Property(x => x.ReferenceType).HasMaxLength(120);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.StatusReason).HasMaxLength(1000);
        builder.Property(x => x.ReversalReason).HasMaxLength(1000);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        builder.HasIndex(x => new { x.TenantId, x.Kind, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.TargetPeriodYear, x.TargetPeriodMonth });
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
        builder.HasIndex(x => x.ReversesTransactionId);
    }
}
```

- [ ] **Step 3: Add the DbSet**

In `backend/src/HR.Infrastructure/Persistence/ApplicationDbContext.cs`, immediately after the `PayrollRunPopulations` DbSet line (~line 259), add:

```csharp
    public DbSet<HR.Domain.Engines.Finance.Entities.PayrollTransaction> PayrollTransactions => Set<HR.Domain.Engines.Finance.Entities.PayrollTransaction>();
```

- [ ] **Step 4: Write the failing persistence test**

Create `backend/tests/HR.Domain.Finance.Tests/PayrollTransactionPersistenceTests.cs`:

```csharp
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTransactionPersistenceTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.Create" };
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    [Fact]
    public async Task Saving_a_transaction_assigns_tenant_audit_and_defaults()
    {
        var name = $"txn-{Guid.NewGuid()}";
        await using (var db = Ctx(name))
        {
            db.PayrollTransactions.Add(new PayrollTransaction
            {
                Kind = PayrollTransactionKind.Deduction,
                EmployeeId = Guid.NewGuid(),
                TypeId = Guid.NewGuid(),
                Amount = 150m,
                TransactionDate = new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc),
                EffectiveDate = new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc),
                TargetPeriodYear = 2026,
                TargetPeriodMonth = 7,
            });
            await db.SaveChangesAsync();
        }

        await using var verify = Ctx(name);
        var saved = await verify.PayrollTransactions.SingleAsync();
        Assert.Equal(PayrollTransactionStatus.Draft, saved.Status);     // default
        Assert.Equal("Manual", saved.SourceModule);                      // default
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), saved.TenantId); // auto tenant
        Assert.NotEqual(default, saved.CreatedAt);                       // auto audit
        Assert.Equal(150m, saved.Amount);
    }
}
```

- [ ] **Step 5: Run the test to verify it fails, then build**

Run: `dotnet test backend/tests/HR.Domain.Finance.Tests --filter PayrollTransactionPersistenceTests`
Expected: FAIL — `PayrollTransactions` not defined / type missing (until steps 1-3 compile).

- [ ] **Step 6: Build, then run the test to verify it passes**

Run: `dotnet build backend/HR.sln` then `dotnet test backend/tests/HR.Domain.Finance.Tests --filter PayrollTransactionPersistenceTests`
Expected: build succeeds; test PASSES.

- [ ] **Step 7: Generate the migration**

Run from `backend/src/HR.Infrastructure`:
```bash
dotnet ef migrations add PayrollTransactions --startup-project ../HR.Api
```
Expected: creates `backend/src/HR.Infrastructure/Migrations/<timestamp>_PayrollTransactions.cs` creating table `engine_payroll_transactions` + the six indexes, and updates `ApplicationDbContextModelSnapshot.cs`. Open the generated migration and confirm it only **creates the new table** (no unexpected drops/alters to existing tables).

- [ ] **Step 8: Commit**

```bash
git add backend/src/HR.Domain/Engines/Finance/Entities/PayrollTransaction.cs backend/src/HR.Infrastructure/Persistence/Configurations/Engines/FinanceConfigurations.cs backend/src/HR.Infrastructure/Persistence/ApplicationDbContext.cs backend/src/HR.Infrastructure/Migrations/ backend/tests/HR.Domain.Finance.Tests/PayrollTransactionPersistenceTests.cs
git commit -m "feat(payroll): PayrollTransaction entity, EF config, DbSet + migration (2A)"
```

---

### Task 3: Application contract — `IPayrollTransactionService` + DTOs

**Files:**
- Create: `backend/src/HR.Application/Engines/Finance/IPayrollTransactionService.cs`

**Interfaces:**
- Consumes: `PayrollTransactionKind`, `PayrollTransactionStatus`.
- Produces: `IPayrollTransactionService` + the records `CreatePayrollTransactionArgs`, `UpdatePayrollTransactionArgs`, `PayrollTransactionFilter`, `PayrollTransactionDto` (namespace `HR.Application.Engines.Finance`). These exact names/types are consumed by Tasks 4 and 5.

- [ ] **Step 1: Write the interface + DTOs**

Create `backend/src/HR.Application/Engines/Finance/IPayrollTransactionService.cs`:

```csharp
using HR.Domain.Enums;

namespace HR.Application.Engines.Finance;

public sealed record CreatePayrollTransactionArgs(
    PayrollTransactionKind Kind,
    Guid EmployeeId,
    Guid TypeId,
    decimal Amount,
    DateTime EffectiveDate,
    DateTime? TransactionDate,
    bool IsRecurring,
    DateTime? RecurrenceEndDate,
    string? Notes,
    Guid? AttachmentFileId,
    bool SubmitImmediately);

public sealed record UpdatePayrollTransactionArgs(
    Guid TypeId,
    decimal Amount,
    DateTime EffectiveDate,
    DateTime? TransactionDate,
    bool IsRecurring,
    DateTime? RecurrenceEndDate,
    string? Notes,
    Guid? AttachmentFileId);

public sealed record PayrollTransactionFilter(
    PayrollTransactionKind? Kind,
    Guid? EmployeeId,
    int? PeriodYear,
    int? PeriodMonth,
    Guid? TypeId,
    PayrollTransactionStatus? Status,
    DateTime? DateFrom,
    DateTime? DateTo);

public sealed record PayrollTransactionDto(
    Guid Id,
    PayrollTransactionKind Kind,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeNumber,
    Guid TypeId,
    string TypeName,
    decimal Amount,
    DateTime TransactionDate,
    DateTime EffectiveDate,
    int? TargetPeriodYear,
    int? TargetPeriodMonth,
    bool IsRecurring,
    DateTime? RecurrenceEndDate,
    string? Notes,
    Guid? AttachmentFileId,
    string SourceModule,
    string? ReferenceType,
    Guid? ReferenceId,
    PayrollTransactionStatus Status,
    string? StatusReason,
    Guid? PayrollRunId,
    DateTime? PostedAt,
    Guid? ReversesTransactionId,
    DateTime CreatedAt);

public interface IPayrollTransactionService
{
    Task<Guid> CreateAsync(CreatePayrollTransactionArgs args, CancellationToken ct);
    Task UpdateAsync(Guid id, UpdatePayrollTransactionArgs args, CancellationToken ct);
    Task<IReadOnlyList<PayrollTransactionDto>> ListAsync(PayrollTransactionFilter filter, CancellationToken ct);
    Task<PayrollTransactionDto?> GetAsync(Guid id, CancellationToken ct);
    Task SubmitAsync(Guid id, CancellationToken ct);
    Task ApproveAsync(Guid id, CancellationToken ct);
    Task RejectAsync(Guid id, string reason, CancellationToken ct);
    Task CancelAsync(Guid id, string? reason, CancellationToken ct);
    Task SetAttachmentAsync(Guid id, Guid fileId, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
```

- [ ] **Step 2: Build to verify it compiles**

Run: `dotnet build backend/src/HR.Application/HR.Application.csproj`
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add backend/src/HR.Application/Engines/Finance/IPayrollTransactionService.cs
git commit -m "feat(payroll): IPayrollTransactionService contract + DTOs (2A)"
```

---

### Task 4: `PayrollTransactionService` implementation + DI registration

**Files:**
- Create: `backend/src/HR.Infrastructure/Engines/Finance/PayrollTransactionService.cs`
- Modify: `backend/src/HR.Infrastructure/DependencyInjection.cs` (add registration after line 63)
- Test: `backend/tests/HR.Domain.Finance.Tests/PayrollTransactionServiceTests.cs`

**Interfaces:**
- Consumes: `IPayrollTransactionService` + records (Task 3); `PayrollTransaction` entity (Task 2); `PayrollTransactionStateMachine` (Task 1); `ApplicationDbContext`; `MasterDataObjectType` (namespace `HR.Domain.Engines.MasterData`); `MasterDataItem`; `Employee` (`HR.Domain.Entities.Employees`).
- Produces: `PayrollTransactionService : IPayrollTransactionService` with ctor `(ApplicationDbContext db, ICurrentUserService user)`.

- [ ] **Step 1: Write the failing service tests**

Create `backend/tests/HR.Domain.Finance.Tests/PayrollTransactionServiceTests.cs`:

```csharp
using FluentAssertions;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.StateMachine;
using HR.Domain.Engines.MasterData;
using HR.Domain.Entities.Employees;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTransactionServiceTests
{
    private static readonly Guid Tenant = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Tenant;
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.Create" };
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    private static PayrollTransactionService Svc(ApplicationDbContext db) => new(db, new FakeUser());

    // Seeds one employee + one DeductionType master-data item, returns their ids.
    private static async Task<(Guid empId, Guid typeId)> SeedAsync(ApplicationDbContext db, string objectType)
    {
        var emp = new Employee { EmployeeNumber = "E1", FirstName = "Ali", LastName = "Saud" };
        db.Employees.Add(emp);
        var type = new MasterDataItem { ObjectType = objectType, Code = "PENALTY", NameAr = "جزاء", NameEn = "Penalty" };
        db.MasterDataItems.Add(type);
        await db.SaveChangesAsync();
        return (emp.Id, type.Id);
    }

    private static CreatePayrollTransactionArgs DeductionArgs(Guid empId, Guid typeId, decimal amount = 100m,
        DateTime? effective = null, bool submit = false) =>
        new(PayrollTransactionKind.Deduction, empId, typeId, amount,
            effective ?? new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc),
            null, false, null, "test", null, submit);

    [Fact]
    public async Task Create_starts_in_draft_and_derives_target_period()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var (empId, typeId) = await SeedAsync(db, MasterDataObjectType.DeductionType);
        var id = await Svc(db).CreateAsync(DeductionArgs(empId, typeId), default);

        var row = await db.PayrollTransactions.SingleAsync(x => x.Id == id);
        row.Status.Should().Be(PayrollTransactionStatus.Draft);
        row.TargetPeriodYear.Should().Be(2026);
        row.TargetPeriodMonth.Should().Be(7);
        row.TransactionDate.Should().Be(row.EffectiveDate); // defaulted
    }

    [Fact]
    public async Task Create_with_submit_goes_to_pending_approval()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var (empId, typeId) = await SeedAsync(db, MasterDataObjectType.DeductionType);
        var id = await Svc(db).CreateAsync(DeductionArgs(empId, typeId, submit: true), default);
        (await db.PayrollTransactions.SingleAsync(x => x.Id == id)).Status
            .Should().Be(PayrollTransactionStatus.PendingApproval);
    }

    [Fact]
    public async Task Create_rejects_negative_amount()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var (empId, typeId) = await SeedAsync(db, MasterDataObjectType.DeductionType);
        var act = () => Svc(db).CreateAsync(DeductionArgs(empId, typeId, amount: -5m), default);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Create_rejects_type_whose_object_type_mismatches_kind()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        // Seed an AdditionType but create a Deduction → mismatch.
        var (empId, typeId) = await SeedAsync(db, MasterDataObjectType.AdditionType);
        var act = () => Svc(db).CreateAsync(DeductionArgs(empId, typeId), default);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Update_is_rejected_once_not_draft()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var (empId, typeId) = await SeedAsync(db, MasterDataObjectType.DeductionType);
        var svc = Svc(db);
        var id = await svc.CreateAsync(DeductionArgs(empId, typeId), default);
        await svc.SubmitAsync(id, default);
        var act = () => svc.UpdateAsync(id, new UpdatePayrollTransactionArgs(typeId, 200m,
            new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc), null, false, null, null, null), default);
        await act.Should().ThrowAsync<InvalidPayrollTransactionStateException>();
    }

    [Fact]
    public async Task Submit_then_approve_advances_status()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var (empId, typeId) = await SeedAsync(db, MasterDataObjectType.DeductionType);
        var svc = Svc(db);
        var id = await svc.CreateAsync(DeductionArgs(empId, typeId), default);
        await svc.SubmitAsync(id, default);
        await svc.ApproveAsync(id, default);
        (await db.PayrollTransactions.SingleAsync(x => x.Id == id)).Status
            .Should().Be(PayrollTransactionStatus.Approved);
    }

    [Fact]
    public async Task Approve_from_draft_is_illegal()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var (empId, typeId) = await SeedAsync(db, MasterDataObjectType.DeductionType);
        var svc = Svc(db);
        var id = await svc.CreateAsync(DeductionArgs(empId, typeId), default);
        var act = () => svc.ApproveAsync(id, default);
        await act.Should().ThrowAsync<InvalidPayrollTransactionStateException>();
    }

    [Fact]
    public async Task Reject_sets_reason_and_status()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var (empId, typeId) = await SeedAsync(db, MasterDataObjectType.DeductionType);
        var svc = Svc(db);
        var id = await svc.CreateAsync(DeductionArgs(empId, typeId, submit: true), default);
        await svc.RejectAsync(id, "missing approval doc", default);
        var row = await db.PayrollTransactions.SingleAsync(x => x.Id == id);
        row.Status.Should().Be(PayrollTransactionStatus.Rejected);
        row.StatusReason.Should().Be("missing approval doc");
    }

    [Fact]
    public async Task List_filters_by_kind_and_resolves_names()
    {
        await using var db = Ctx($"t-{Guid.NewGuid()}");
        var (empId, typeId) = await SeedAsync(db, MasterDataObjectType.DeductionType);
        var svc = Svc(db);
        await svc.CreateAsync(DeductionArgs(empId, typeId), default);

        var rows = await svc.ListAsync(new PayrollTransactionFilter(
            PayrollTransactionKind.Deduction, null, null, null, null, null, null, null), default);
        rows.Should().HaveCount(1);
        rows[0].EmployeeName.Should().Contain("Ali");
        rows[0].TypeName.Should().Be("جزاء");

        var none = await svc.ListAsync(new PayrollTransactionFilter(
            PayrollTransactionKind.Addition, null, null, null, null, null, null, null), default);
        none.Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test backend/tests/HR.Domain.Finance.Tests --filter PayrollTransactionServiceTests`
Expected: FAIL — `PayrollTransactionService` does not exist.

- [ ] **Step 3: Write the service**

Create `backend/src/HR.Infrastructure/Engines/Finance/PayrollTransactionService.cs`:

```csharp
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Engines.Finance.StateMachine;
using HR.Domain.Engines.MasterData;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

public sealed class PayrollTransactionService : IPayrollTransactionService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public PayrollTransactionService(ApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Guid> CreateAsync(CreatePayrollTransactionArgs args, CancellationToken ct)
    {
        if (args.Amount < 0) throw new InvalidOperationException("Amount must be non-negative.");
        await EnsureEmployeeAsync(args.EmployeeId, ct);
        await EnsureTypeMatchesKindAsync(args.TypeId, args.Kind, ct);

        var effective = args.EffectiveDate;
        var txn = new PayrollTransaction
        {
            Kind = args.Kind,
            EmployeeId = args.EmployeeId,
            TypeId = args.TypeId,
            Amount = args.Amount,
            EffectiveDate = effective,
            TransactionDate = args.TransactionDate ?? effective,
            TargetPeriodYear = effective.Year,
            TargetPeriodMonth = effective.Month,
            IsRecurring = args.IsRecurring,
            RecurrenceEndDate = args.RecurrenceEndDate,
            Notes = args.Notes,
            AttachmentFileId = args.AttachmentFileId,
            SourceModule = "Manual",
            Status = args.SubmitImmediately ? PayrollTransactionStatus.PendingApproval : PayrollTransactionStatus.Draft,
        };
        _db.PayrollTransactions.Add(txn);
        await _db.SaveChangesAsync(ct);
        return txn.Id;
    }

    public async Task UpdateAsync(Guid id, UpdatePayrollTransactionArgs args, CancellationToken ct)
    {
        if (args.Amount < 0) throw new InvalidOperationException("Amount must be non-negative.");
        var txn = await GetTrackedAsync(id, ct);
        if (!PayrollTransactionStateMachine.IsEditable(txn.Status))
            throw new InvalidPayrollTransactionStateException(txn.Status, txn.Status);
        await EnsureTypeMatchesKindAsync(args.TypeId, txn.Kind, ct);

        txn.TypeId = args.TypeId;
        txn.Amount = args.Amount;
        txn.EffectiveDate = args.EffectiveDate;
        txn.TransactionDate = args.TransactionDate ?? args.EffectiveDate;
        txn.TargetPeriodYear = args.EffectiveDate.Year;
        txn.TargetPeriodMonth = args.EffectiveDate.Month;
        txn.IsRecurring = args.IsRecurring;
        txn.RecurrenceEndDate = args.RecurrenceEndDate;
        txn.Notes = args.Notes;
        txn.AttachmentFileId = args.AttachmentFileId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task SubmitAsync(Guid id, CancellationToken ct) =>
        await TransitionAsync(id, PayrollTransactionStatus.PendingApproval, null, ct);

    public async Task ApproveAsync(Guid id, CancellationToken ct) =>
        await TransitionAsync(id, PayrollTransactionStatus.Approved, null, ct);

    public async Task RejectAsync(Guid id, string reason, CancellationToken ct) =>
        await TransitionAsync(id, PayrollTransactionStatus.Rejected, reason, ct);

    public async Task CancelAsync(Guid id, string? reason, CancellationToken ct) =>
        await TransitionAsync(id, PayrollTransactionStatus.Cancelled, reason, ct);

    public async Task SetAttachmentAsync(Guid id, Guid fileId, CancellationToken ct)
    {
        var txn = await GetTrackedAsync(id, ct);
        if (PayrollTransactionStateMachine.IsImmutable(txn.Status))
            throw new InvalidPayrollTransactionStateException(txn.Status, txn.Status);
        txn.AttachmentFileId = fileId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var txn = await GetTrackedAsync(id, ct);
        if (txn.Status != PayrollTransactionStatus.Draft)
            throw new InvalidPayrollTransactionStateException(txn.Status, PayrollTransactionStatus.Draft);
        txn.IsDeleted = true;
        txn.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PayrollTransactionDto?> GetAsync(Guid id, CancellationToken ct)
    {
        var rows = await Query().Where(x => x.txn.Id == id).ToListAsync(ct);
        return rows.Select(Project).FirstOrDefault();
    }

    public async Task<IReadOnlyList<PayrollTransactionDto>> ListAsync(PayrollTransactionFilter f, CancellationToken ct)
    {
        var q = Query();
        if (f.Kind is not null) q = q.Where(x => x.txn.Kind == f.Kind);
        if (f.EmployeeId is not null) q = q.Where(x => x.txn.EmployeeId == f.EmployeeId);
        if (f.PeriodYear is not null) q = q.Where(x => x.txn.TargetPeriodYear == f.PeriodYear);
        if (f.PeriodMonth is not null) q = q.Where(x => x.txn.TargetPeriodMonth == f.PeriodMonth);
        if (f.TypeId is not null) q = q.Where(x => x.txn.TypeId == f.TypeId);
        if (f.Status is not null) q = q.Where(x => x.txn.Status == f.Status);
        if (f.DateFrom is not null) q = q.Where(x => x.txn.EffectiveDate >= f.DateFrom);
        if (f.DateTo is not null) q = q.Where(x => x.txn.EffectiveDate <= f.DateTo);

        var rows = await q.OrderByDescending(x => x.txn.CreatedAt).ToListAsync(ct);
        return rows.Select(Project).ToList();
    }

    // --- helpers ---

    private async Task TransitionAsync(Guid id, PayrollTransactionStatus to, string? reason, CancellationToken ct)
    {
        var txn = await GetTrackedAsync(id, ct);
        PayrollTransactionStateMachine.EnsureCanTransition(txn.Status, to);
        txn.Status = to;
        if (reason is not null) txn.StatusReason = reason;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<PayrollTransaction> GetTrackedAsync(Guid id, CancellationToken ct) =>
        await _db.PayrollTransactions.FirstOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new InvalidOperationException($"Payroll transaction {id} not found.");

    private async Task EnsureEmployeeAsync(Guid employeeId, CancellationToken ct)
    {
        var exists = await _db.Employees.AnyAsync(e => e.Id == employeeId, ct);
        if (!exists) throw new InvalidOperationException($"Employee {employeeId} not found.");
    }

    private async Task EnsureTypeMatchesKindAsync(Guid typeId, PayrollTransactionKind kind, CancellationToken ct)
    {
        var expected = kind == PayrollTransactionKind.Addition
            ? MasterDataObjectType.AdditionType : MasterDataObjectType.DeductionType;
        var item = await _db.MasterDataItems.FirstOrDefaultAsync(x => x.Id == typeId, ct)
            ?? throw new InvalidOperationException($"Type {typeId} not found.");
        if (!string.Equals(item.ObjectType, expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Type {typeId} is '{item.ObjectType}', expected '{expected}'.");
    }

    // Join transaction → employee + type so the DTO carries display names.
    private IQueryable<Row> Query() =>
        from txn in _db.PayrollTransactions
        join emp in _db.Employees on txn.EmployeeId equals emp.Id into empJ
        from emp in empJ.DefaultIfEmpty()
        join type in _db.MasterDataItems on txn.TypeId equals type.Id into typeJ
        from type in typeJ.DefaultIfEmpty()
        select new Row { txn = txn, emp = emp, type = type };

    private sealed class Row
    {
        public PayrollTransaction txn = null!;
        public HR.Domain.Entities.Employees.Employee? emp;
        public MasterDataItem? type;
    }

    private static PayrollTransactionDto Project(Row r) => new(
        r.txn.Id, r.txn.Kind, r.txn.EmployeeId,
        EmployeeName(r.emp), r.emp?.EmployeeNumber ?? "",
        r.txn.TypeId, r.type?.NameAr ?? r.type?.NameEn ?? "",
        r.txn.Amount, r.txn.TransactionDate, r.txn.EffectiveDate,
        r.txn.TargetPeriodYear, r.txn.TargetPeriodMonth, r.txn.IsRecurring, r.txn.RecurrenceEndDate,
        r.txn.Notes, r.txn.AttachmentFileId, r.txn.SourceModule, r.txn.ReferenceType, r.txn.ReferenceId,
        r.txn.Status, r.txn.StatusReason, r.txn.PayrollRunId, r.txn.PostedAt,
        r.txn.ReversesTransactionId, r.txn.CreatedAt);

    private static string EmployeeName(HR.Domain.Entities.Employees.Employee? e)
    {
        if (e is null) return "";
        var first = string.IsNullOrWhiteSpace(e.FirstNameAr) ? e.FirstName : e.FirstNameAr!;
        var last = string.IsNullOrWhiteSpace(e.LastNameAr) ? e.LastName : e.LastNameAr!;
        return $"{first} {last}".Trim();
    }
}
```

> NOTE: soft-deleted rows are excluded by the global query filter on `IsDeleted`. If the Finance area does not define that filter globally, the `Query()`/`GetTrackedAsync` reads will still include deleted rows — confirm by checking `ApplicationDbContext` for a soft-delete `HasQueryFilter`. The persistence test in Task 2 and service tests here do not delete, so they are unaffected.

- [ ] **Step 4: Register the service in DI**

In `backend/src/HR.Infrastructure/DependencyInjection.cs`, immediately after line 63 (`services.AddScoped<...IPayrollTypeService, ...PayrollTypeService>();`), add:

```csharp
        services.AddScoped<HR.Application.Engines.Finance.IPayrollTransactionService, HR.Infrastructure.Engines.Finance.PayrollTransactionService>();
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test backend/tests/HR.Domain.Finance.Tests --filter PayrollTransactionServiceTests`
Expected: all PASS.

- [ ] **Step 6: Commit**

```bash
git add backend/src/HR.Infrastructure/Engines/Finance/PayrollTransactionService.cs backend/src/HR.Infrastructure/DependencyInjection.cs backend/tests/HR.Domain.Finance.Tests/PayrollTransactionServiceTests.cs
git commit -m "feat(payroll): PayrollTransactionService (create/validate/lifecycle/list) + DI (2A)"
```

---

### Task 5: Controller endpoints + request/response DTOs

**Files:**
- Create: `backend/src/HR.Modules/Payroll/DTOs/PayrollTransactionDtos.cs`
- Modify: `backend/src/HR.Modules/Payroll/Controllers/PayrollController.cs` (inject service; add endpoints)

**Interfaces:**
- Consumes: `IPayrollTransactionService` + records/DTOs (Task 3).
- Produces: REST endpoints under `api/payroll/transactions`.

- [ ] **Step 1: Write the request DTOs**

Create `backend/src/HR.Modules/Payroll/DTOs/PayrollTransactionDtos.cs`:

```csharp
using HR.Domain.Enums;

namespace HR.Modules.Payroll.DTOs;

public sealed class CreateTransactionRequest
{
    public PayrollTransactionKind Kind { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid TypeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? TransactionDate { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
    public string? Notes { get; set; }
    public Guid? AttachmentFileId { get; set; }
    public bool SubmitImmediately { get; set; }
}

public sealed class UpdateTransactionRequest
{
    public Guid TypeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? TransactionDate { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
    public string? Notes { get; set; }
    public Guid? AttachmentFileId { get; set; }
}

public sealed class RejectTransactionRequest { public string Reason { get; set; } = ""; }
public sealed class CancelTransactionRequest { public string? Reason { get; set; } }
public sealed class SetAttachmentRequest { public Guid FileId { get; set; } }
```

- [ ] **Step 2: Inject the service into the controller**

In `backend/src/HR.Modules/Payroll/Controllers/PayrollController.cs`, add a field and constructor parameter. Add after the `private readonly IScopeEngine _scope;` field:

```csharp
    private readonly IPayrollTransactionService _transactions;
```

Update the constructor signature and body to include the new dependency (append `IPayrollTransactionService transactions` to the parameter list and `_transactions = transactions;` to the body):

```csharp
    public PayrollController(ApplicationDbContext db, IPayrollRunEngine runEngine, IPayrollPreviewEngine previewEngine,
        IPayrollExecutionScheduler scheduler, IStandardPayrollSeeder seeder,
        IPayrollTypeService types, IScopeEngine scope, IPayrollTransactionService transactions)
    {
        _db = db;
        _runEngine = runEngine;
        _previewEngine = previewEngine;
        _scheduler = scheduler;
        _seeder = seeder;
        _types = types;
        _scope = scope;
        _transactions = transactions;
    }
```

(`IPayrollTransactionService` is in `HR.Application.Engines.Finance`, already imported by the controller's `using HR.Application.Engines.Finance;`.)

- [ ] **Step 3: Add the endpoints**

Add these methods inside the `PayrollController` class (e.g. after the scope endpoints). `PayrollTransactionFilter`/`CreatePayrollTransactionArgs`/`UpdatePayrollTransactionArgs` come from `HR.Application.Engines.Finance`; the request DTOs from `HR.Modules.Payroll.DTOs` (already imported):

```csharp
    [HttpGet("transactions")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<List<PayrollTransactionDto>>>> ListTransactions(
        [FromQuery] PayrollTransactionKind? kind, [FromQuery] Guid? employeeId,
        [FromQuery] int? periodYear, [FromQuery] int? periodMonth, [FromQuery] Guid? typeId,
        [FromQuery] PayrollTransactionStatus? status, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo,
        CancellationToken ct)
    {
        var rows = await _transactions.ListAsync(
            new PayrollTransactionFilter(kind, employeeId, periodYear, periodMonth, typeId, status, dateFrom, dateTo), ct);
        return OkResponse(rows.ToList());
    }

    [HttpGet("transactions/{id:guid}")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<PayrollTransactionDto>>> GetTransaction(Guid id, CancellationToken ct)
    {
        var dto = await _transactions.GetAsync(id, ct);
        return dto is null ? NotFound(ApiResponse<PayrollTransactionDto>.Fail("غير موجود")) : OkResponse(dto);
    }

    [HttpPost("transactions")]
    [RequirePermission("Payroll.Create")]
    public async Task<ActionResult<ApiResponse<PayrollTransactionDto>>> CreateTransaction(
        [FromBody] CreateTransactionRequest req, CancellationToken ct)
    {
        var id = await _transactions.CreateAsync(new CreatePayrollTransactionArgs(
            req.Kind, req.EmployeeId, req.TypeId, req.Amount, req.EffectiveDate, req.TransactionDate,
            req.IsRecurring, req.RecurrenceEndDate, req.Notes, req.AttachmentFileId, req.SubmitImmediately), ct);
        return CreatedResponse(await _transactions.GetAsync(id, ct));
    }

    [HttpPut("transactions/{id:guid}")]
    [RequirePermission("Payroll.Edit")]
    public async Task<ActionResult<ApiResponse<PayrollTransactionDto>>> UpdateTransaction(
        Guid id, [FromBody] UpdateTransactionRequest req, CancellationToken ct)
    {
        await _transactions.UpdateAsync(id, new UpdatePayrollTransactionArgs(
            req.TypeId, req.Amount, req.EffectiveDate, req.TransactionDate,
            req.IsRecurring, req.RecurrenceEndDate, req.Notes, req.AttachmentFileId), ct);
        return OkResponse(await _transactions.GetAsync(id, ct));
    }

    [HttpPost("transactions/{id:guid}/submit")]
    [RequirePermission("Payroll.Edit")]
    public async Task<ActionResult<ApiResponse<PayrollTransactionDto>>> SubmitTransaction(Guid id, CancellationToken ct)
    {
        await _transactions.SubmitAsync(id, ct);
        return OkResponse(await _transactions.GetAsync(id, ct));
    }

    [HttpPost("transactions/{id:guid}/approve")]
    [RequirePermission("Payroll.Approve")]
    public async Task<ActionResult<ApiResponse<PayrollTransactionDto>>> ApproveTransaction(Guid id, CancellationToken ct)
    {
        await _transactions.ApproveAsync(id, ct);
        return OkResponse(await _transactions.GetAsync(id, ct));
    }

    [HttpPost("transactions/{id:guid}/reject")]
    [RequirePermission("Payroll.Approve")]
    public async Task<ActionResult<ApiResponse<PayrollTransactionDto>>> RejectTransaction(
        Guid id, [FromBody] RejectTransactionRequest req, CancellationToken ct)
    {
        await _transactions.RejectAsync(id, req.Reason, ct);
        return OkResponse(await _transactions.GetAsync(id, ct));
    }

    [HttpPost("transactions/{id:guid}/cancel")]
    [RequirePermission("Payroll.Edit")]
    public async Task<ActionResult<ApiResponse<PayrollTransactionDto>>> CancelTransaction(
        Guid id, [FromBody] CancelTransactionRequest req, CancellationToken ct)
    {
        await _transactions.CancelAsync(id, req.Reason, ct);
        return OkResponse(await _transactions.GetAsync(id, ct));
    }

    [HttpPost("transactions/{id:guid}/attachment")]
    [RequirePermission("Payroll.Edit")]
    public async Task<ActionResult<ApiResponse<PayrollTransactionDto>>> SetTransactionAttachment(
        Guid id, [FromBody] SetAttachmentRequest req, CancellationToken ct)
    {
        await _transactions.SetAttachmentAsync(id, req.FileId, ct);
        return OkResponse(await _transactions.GetAsync(id, ct));
    }

    [HttpDelete("transactions/{id:guid}")]
    [RequirePermission("Payroll.Delete")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteTransaction(Guid id, CancellationToken ct)
    {
        await _transactions.DeleteAsync(id, ct);
        return OkResponse<object>(new { deleted = true });
    }
```

> NOTE: confirm the `BaseApiController` helper names by reading `backend/src/HR.Api/Controllers/BaseApiController.cs` — this plan assumes `OkResponse(...)`, `CreatedResponse(...)`, and that `ApiResponse<T>.Fail(string)` exists (both are used by existing controllers). If `OkResponse<T>(object)` generic form differs, mirror the exact signature used by the `runs` endpoints.

- [ ] **Step 4: Build the whole solution**

Run: `dotnet build backend/HR.sln`
Expected: PASS (no controller test project exists; this endpoint set is verified manually in Task 8).

- [ ] **Step 5: Commit**

```bash
git add backend/src/HR.Modules/Payroll/DTOs/PayrollTransactionDtos.cs backend/src/HR.Modules/Payroll/Controllers/PayrollController.cs
git commit -m "feat(payroll): transaction CRUD/approve/cancel endpoints on api/payroll (2A)"
```

---

### Task 6: Frontend api wrapper

**Files:**
- Create: `src/lib/api/payroll-transactions.ts`

**Interfaces:**
- Consumes: `apiFetch` from `../api-client`.
- Produces: types `PayrollTransaction`, `TransactionKind`, `TransactionStatus`, `CreateTransactionInput`, `UpdateTransactionInput`, `TransactionFilter`; functions `listTransactions`, `getTransaction`, `createTransaction`, `updateTransaction`, `submitTransaction`, `approveTransaction`, `rejectTransaction`, `cancelTransaction`, `setTransactionAttachment`, `deleteTransaction`.

- [ ] **Step 1: Write the wrapper**

Create `src/lib/api/payroll-transactions.ts`:

```ts
import { apiFetch } from "../api-client";

// Mirror of backend PayrollTransactionKind / PayrollTransactionStatus (int enums serialized as numbers).
export type TransactionKind = 1 | 2; // 1 = Addition, 2 = Deduction
export type TransactionStatus = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7;
// 0 Draft · 1 PendingApproval · 2 Approved · 3 Rejected · 4 Cancelled · 5 CarriedForward · 6 Posted · 7 Reversed

export interface PayrollTransaction {
  id: string;
  kind: TransactionKind;
  employeeId: string;
  employeeName: string;
  employeeNumber: string;
  typeId: string;
  typeName: string;
  amount: number;
  transactionDate: string;
  effectiveDate: string;
  targetPeriodYear: number | null;
  targetPeriodMonth: number | null;
  isRecurring: boolean;
  recurrenceEndDate: string | null;
  notes: string | null;
  attachmentFileId: string | null;
  sourceModule: string;
  referenceType: string | null;
  referenceId: string | null;
  status: TransactionStatus;
  statusReason: string | null;
  payrollRunId: string | null;
  postedAt: string | null;
  reversesTransactionId: string | null;
  createdAt: string;
}

export interface CreateTransactionInput {
  kind: TransactionKind;
  employeeId: string;
  typeId: string;
  amount: number;
  effectiveDate: string;          // ISO date
  transactionDate?: string | null;
  isRecurring: boolean;
  recurrenceEndDate?: string | null;
  notes?: string | null;
  attachmentFileId?: string | null;
  submitImmediately: boolean;
}

export type UpdateTransactionInput = Omit<CreateTransactionInput, "kind" | "employeeId" | "submitImmediately">;

export interface TransactionFilter {
  kind?: TransactionKind;
  employeeId?: string;
  periodYear?: number;
  periodMonth?: number;
  typeId?: string;
  status?: TransactionStatus;
  dateFrom?: string;
  dateTo?: string;
}

const BASE = "/api/payroll/transactions";

export async function listTransactions(filter: TransactionFilter = {}): Promise<PayrollTransaction[]> {
  const q = new URLSearchParams();
  if (filter.kind != null) q.set("kind", String(filter.kind));
  if (filter.employeeId) q.set("employeeId", filter.employeeId);
  if (filter.periodYear != null) q.set("periodYear", String(filter.periodYear));
  if (filter.periodMonth != null) q.set("periodMonth", String(filter.periodMonth));
  if (filter.typeId) q.set("typeId", filter.typeId);
  if (filter.status != null) q.set("status", String(filter.status));
  if (filter.dateFrom) q.set("dateFrom", filter.dateFrom);
  if (filter.dateTo) q.set("dateTo", filter.dateTo);
  const qs = q.toString();
  return (await apiFetch<PayrollTransaction[]>(`${BASE}${qs ? `?${qs}` : ""}`)) ?? [];
}

export function getTransaction(id: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}`);
}

export function createTransaction(input: CreateTransactionInput): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(BASE, { method: "POST", body: input });
}

export function updateTransaction(id: string, input: UpdateTransactionInput): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}`, { method: "PUT", body: input });
}

export function submitTransaction(id: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}/submit`, { method: "POST" });
}

export function approveTransaction(id: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}/approve`, { method: "POST" });
}

export function rejectTransaction(id: string, reason: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}/reject`, { method: "POST", body: { reason } });
}

export function cancelTransaction(id: string, reason?: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}/cancel`, { method: "POST", body: { reason } });
}

export function setTransactionAttachment(id: string, fileId: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}/attachment`, { method: "POST", body: { fileId } });
}

export function deleteTransaction(id: string): Promise<void> {
  return apiFetch<void>(`${BASE}/${id}`, { method: "DELETE" });
}

export const TRANSACTION_STATUS_AR: Record<TransactionStatus, string> = {
  0: "مسودة", 1: "بانتظار الاعتماد", 2: "معتمد", 3: "مرفوض",
  4: "ملغى", 5: "مُرحّل", 6: "مُرحّل للسجل", 7: "معكوس",
};
```

- [ ] **Step 2: Type-check the frontend**

Run: `npx tsc --noEmit` (from repo root)
Expected: no new type errors in `src/lib/api/payroll-transactions.ts`.

- [ ] **Step 3: Commit**

```bash
git add src/lib/api/payroll-transactions.ts
git commit -m "feat(payroll-ui): payroll-transactions api wrapper (2A)"
```

---

### Task 7: Frontend pages — shared component + two routes + nav

**Files:**
- Create: `src/components/payroll/transaction-status-badge.tsx`
- Create: `src/components/payroll/transactions-page.tsx`
- Create: `src/app/(dashboard)/payroll/additions/page.tsx`
- Create: `src/app/(dashboard)/payroll/deductions/page.tsx`
- Modify: `src/app/(dashboard)/payroll/page.tsx` (add nav links to the two new pages)

**Interfaces:**
- Consumes: the Task 6 wrapper; `getEmployees` from `@/lib/api/employees`; `getMasterDataItems` from `@/lib/api/master-data`; `uploadFile` from `@/lib/api/files`; `usePermissions`/`AccessGuard`; `Combobox`, `Dialog`, `Table`, `Button`, `Input`, `Label`, `Badge`; `toast` from `sonner`.

- [ ] **Step 1: Write the status badge**

Create `src/components/payroll/transaction-status-badge.tsx`:

```tsx
import { Badge } from "@/components/ui/badge";
import { TransactionStatus, TRANSACTION_STATUS_AR } from "@/lib/api/payroll-transactions";

const STYLES: Record<TransactionStatus, string> = {
  0: "bg-zinc-500/10 text-zinc-400 border-zinc-500/20",     // Draft
  1: "bg-amber-500/10 text-amber-500 border-amber-500/20",  // PendingApproval
  2: "bg-green-500/10 text-green-500 border-green-500/20",   // Approved
  3: "bg-red-500/10 text-red-500 border-red-500/20",         // Rejected
  4: "bg-zinc-500/10 text-zinc-400 border-zinc-500/20",      // Cancelled
  5: "bg-blue-500/10 text-blue-500 border-blue-500/20",      // CarriedForward
  6: "bg-violet-500/10 text-violet-500 border-violet-500/20",// Posted
  7: "bg-orange-500/10 text-orange-500 border-orange-500/20",// Reversed
};

export function TransactionStatusBadge({ status }: { status: TransactionStatus }) {
  return (
    <Badge variant="outline" className={`text-xs ${STYLES[status]}`}>
      {TRANSACTION_STATUS_AR[status]}
    </Badge>
  );
}
```

- [ ] **Step 2: Write the shared transactions page component**

Create `src/components/payroll/transactions-page.tsx`. This is the full list + create/edit dialog + lifecycle actions component, parameterized by `kind`. It mirrors the allowances page structure (Table + Dialog + toast + `notifyError`).

```tsx
"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Pencil, Trash2, Loader2, Send, Check, X, Ban, Paperclip } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { ApiError } from "@/lib/api-client";
import { AccessGuard } from "@/components/access/access-guard";
import { usePermissions } from "@/lib/permissions";
import { TransactionStatusBadge } from "@/components/payroll/transaction-status-badge";
import { getEmployees, type Employee } from "@/lib/api/employees";
import { getMasterDataItems, type MasterDataItem } from "@/lib/api/master-data";
import { uploadFile } from "@/lib/api/files";
import {
  listTransactions, createTransaction, updateTransaction, submitTransaction, approveTransaction,
  rejectTransaction, cancelTransaction, setTransactionAttachment, deleteTransaction,
  type PayrollTransaction, type TransactionKind,
} from "@/lib/api/payroll-transactions";

const MONTHS_AR = ["", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"];

function notifyError(err: unknown, fallback: string) {
  if (!(err instanceof ApiError) || ![401, 403, 500].includes(err.status)) {
    toast.error(err instanceof ApiError ? err.message : fallback);
  }
}

function todayIso() { return new Date().toISOString().slice(0, 10); }

interface Copy { title: string; subtitle: string; one: string; back: string; objectType: string; }

const COPY: Record<TransactionKind, Copy> = {
  1: { title: "الإضافات", subtitle: "إضافات الرواتب — مكافآت، عمولات، بدلات لمرة واحدة", one: "إضافة", back: "الرواتب", objectType: "AdditionType" },
  2: { title: "الاستقطاعات", subtitle: "استقطاعات الرواتب — جزاءات، خصومات، تسويات", one: "استقطاع", back: "الرواتب", objectType: "DeductionType" },
};

interface Form {
  employeeId: string | null;
  typeId: string | null;
  amount: string;
  effectiveDate: string;
  transactionDate: string;
  notes: string;
  isRecurring: boolean;
}

const emptyForm: Form = {
  employeeId: null, typeId: null, amount: "", effectiveDate: todayIso(), transactionDate: "", notes: "", isRecurring: false,
};

export function TransactionsPage({ kind }: { kind: TransactionKind }) {
  return <AccessGuard anyOf={["Payroll.View"]}><Inner kind={kind} /></AccessGuard>;
}

function Inner({ kind }: { kind: TransactionKind }) {
  const copy = COPY[kind];
  const { has } = usePermissions();
  const canCreate = has("Payroll.Create");
  const canEdit = has("Payroll.Edit");
  const canApprove = has("Payroll.Approve");
  const canDelete = has("Payroll.Delete");

  const [rows, setRows] = useState<PayrollTransaction[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [types, setTypes] = useState<MasterDataItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<PayrollTransaction | null>(null);
  const [form, setForm] = useState<Form>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<PayrollTransaction | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try { setRows(await listTransactions({ kind })); }
    catch (err) { notifyError(err, "تعذر تحميل البيانات"); }
    finally { setLoading(false); }
  }, [kind]);

  useEffect(() => { load(); }, [load]);
  useEffect(() => {
    (async () => {
      try {
        const [emps, tps] = await Promise.all([getEmployees(), getMasterDataItems(copy.objectType)]);
        setEmployees(emps); setTypes(tps);
      } catch (err) { notifyError(err, "تعذر تحميل القوائم"); }
    })();
  }, [copy.objectType]);

  const empOptions = useMemo(() => employees.map((e) => ({ value: e.id, label: e.name })), [employees]);
  const typeOptions = useMemo(() => types.map((t) => ({ value: t.id, label: t.nameAr || t.nameEn })), [types]);

  function openCreate() { setEditing(null); setForm(emptyForm); setDialogOpen(true); }
  function openEdit(t: PayrollTransaction) {
    setEditing(t);
    setForm({
      employeeId: t.employeeId, typeId: t.typeId, amount: String(t.amount),
      effectiveDate: t.effectiveDate.slice(0, 10),
      transactionDate: t.transactionDate ? t.transactionDate.slice(0, 10) : "",
      notes: t.notes ?? "", isRecurring: t.isRecurring,
    });
    setDialogOpen(true);
  }

  async function save(submit: boolean) {
    if (!form.employeeId) { toast.error("اختر موظفاً"); return; }
    if (!form.typeId) { toast.error("اختر النوع"); return; }
    const amount = Number(form.amount);
    if (Number.isNaN(amount) || amount < 0) { toast.error("المبلغ غير صالح"); return; }
    if (!form.effectiveDate) { toast.error("تاريخ السريان مطلوب"); return; }
    setSaving(true);
    try {
      if (editing) {
        await updateTransaction(editing.id, {
          typeId: form.typeId, amount, effectiveDate: form.effectiveDate,
          transactionDate: form.transactionDate || null, isRecurring: form.isRecurring,
          recurrenceEndDate: null, notes: form.notes.trim() || null, attachmentFileId: editing.attachmentFileId,
        });
        if (submit) await submitTransaction(editing.id);
        toast.success("تم الحفظ");
      } else {
        await createTransaction({
          kind, employeeId: form.employeeId, typeId: form.typeId, amount,
          effectiveDate: form.effectiveDate, transactionDate: form.transactionDate || null,
          isRecurring: form.isRecurring, recurrenceEndDate: null, notes: form.notes.trim() || null,
          attachmentFileId: null, submitImmediately: submit,
        });
        toast.success("تمت الإضافة");
      }
      setDialogOpen(false); await load();
    } catch (err) { notifyError(err, "تعذر الحفظ"); } finally { setSaving(false); }
  }

  async function act(id: string, fn: () => Promise<unknown>, ok: string) {
    setBusyId(id);
    try { await fn(); toast.success(ok); await load(); }
    catch (err) { notifyError(err, "تعذر تنفيذ الإجراء"); } finally { setBusyId(null); }
  }

  async function doReject(t: PayrollTransaction) {
    const reason = window.prompt("سبب الرفض؟");
    if (reason == null) return;
    await act(t.id, () => rejectTransaction(t.id, reason), "تم الرفض");
  }

  async function attach(t: PayrollTransaction, file: File) {
    setBusyId(t.id);
    try {
      const up = await uploadFile(file, "payroll");
      await setTransactionAttachment(t.id, up.id);
      toast.success("تم إرفاق الملف"); await load();
    } catch (err) { notifyError(err, "تعذر الإرفاق"); } finally { setBusyId(null); }
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    await act(deleteTarget.id, () => deleteTransaction(deleteTarget.id), "تم الحذف");
    setDeleteTarget(null);
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href="/payroll" className="text-muted-foreground hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowRight className="h-4 w-4" /> {copy.back}
        </Link>
        <span className="text-muted-foreground">/</span>
        <span>{copy.title}</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{copy.title}</h1>
          <p className="text-sm text-muted-foreground mt-1">{copy.subtitle}</p>
        </div>
        {canCreate && (
          <Button onClick={openCreate} className="h-10 gap-2 font-bold text-sm"><Plus className="h-4 w-4" /> {copy.one}</Button>
        )}
      </div>

      <div className="border border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-transparent">
              {["الموظف", "النوع", "المبلغ", "تاريخ السريان", "سيؤثر على", "الحالة", ""].map((h, i) => (
                <TableHead key={i} className="text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">{h}</TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={7} className="py-12 text-center text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin inline" /> جاري التحميل...</TableCell></TableRow>
            ) : rows.length === 0 ? (
              <TableRow className="hover:bg-transparent"><TableCell colSpan={7} className="py-12 text-center text-sm text-muted-foreground">لا توجد سجلات</TableCell></TableRow>
            ) : rows.map((t) => (
              <TableRow key={t.id} className="border-border hover:bg-card/50">
                <TableCell><div className="font-medium">{t.employeeName}</div><div className="font-mono text-[10px] text-muted-foreground">{t.employeeNumber}</div></TableCell>
                <TableCell className="text-sm">{t.typeName}</TableCell>
                <TableCell className="text-sm tabular-nums">{t.amount.toLocaleString()}</TableCell>
                <TableCell className="text-sm text-muted-foreground" dir="ltr">{t.effectiveDate.slice(0, 10)}</TableCell>
                <TableCell className="text-sm text-muted-foreground">
                  {t.targetPeriodYear ? `${MONTHS_AR[t.targetPeriodMonth ?? 0]} ${t.targetPeriodYear}` : "—"}
                </TableCell>
                <TableCell><TransactionStatusBadge status={t.status} /></TableCell>
                <TableCell>
                  <div className="flex items-center gap-1 justify-end">
                    {busyId === t.id && <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />}
                    {canEdit && t.status === 0 && (
                      <>
                        <button onClick={() => openEdit(t)} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="تعديل"><Pencil className="h-4 w-4" /></button>
                        <button onClick={() => act(t.id, () => submitTransaction(t.id), "تم الإرسال للاعتماد")} className="h-8 w-8 inline-flex items-center justify-center text-amber-500 hover:text-amber-400" title="إرسال للاعتماد"><Send className="h-4 w-4" /></button>
                      </>
                    )}
                    {canApprove && t.status === 1 && (
                      <>
                        <button onClick={() => act(t.id, () => approveTransaction(t.id), "تم الاعتماد")} className="h-8 w-8 inline-flex items-center justify-center text-green-500 hover:text-green-400" title="اعتماد"><Check className="h-4 w-4" /></button>
                        <button onClick={() => doReject(t)} className="h-8 w-8 inline-flex items-center justify-center text-red-500 hover:text-red-400" title="رفض"><X className="h-4 w-4" /></button>
                      </>
                    )}
                    {canEdit && (t.status === 0 || t.status === 2) && (
                      <button onClick={() => act(t.id, () => cancelTransaction(t.id), "تم الإلغاء")} className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground" title="إلغاء"><Ban className="h-4 w-4" /></button>
                    )}
                    {canEdit && t.status !== 6 && t.status !== 7 && (
                      <label className="h-8 w-8 inline-flex items-center justify-center text-muted-foreground hover:text-foreground cursor-pointer" title="إرفاق ملف">
                        <Paperclip className="h-4 w-4" />
                        <input type="file" className="hidden" onChange={(e) => { const f = e.target.files?.[0]; if (f) attach(t, f); e.currentTarget.value = ""; }} />
                      </label>
                    )}
                    {canDelete && t.status === 0 && (
                      <button onClick={() => setDeleteTarget(t)} className="h-8 w-8 inline-flex items-center justify-center text-destructive hover:text-destructive/80" title="حذف"><Trash2 className="h-4 w-4" /></button>
                    )}
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <Dialog open={dialogOpen} onOpenChange={(o) => { if (!o && !saving) setDialogOpen(false); }}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader><DialogTitle>{editing ? `تعديل ${copy.one}` : `${copy.one} جديد`}</DialogTitle></DialogHeader>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2">
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">الموظف</Label>
              <Combobox value={form.employeeId} onChange={(v) => setForm({ ...form, employeeId: v })} options={empOptions} placeholder="اختر موظفاً…" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">النوع</Label>
              <Combobox value={form.typeId} onChange={(v) => setForm({ ...form, typeId: v })} options={typeOptions} placeholder="اختر النوع…" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">المبلغ</Label>
              <Input type="number" step="any" min={0} value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} className="bg-secondary border-border" dir="ltr" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">تاريخ السريان</Label>
              <Input type="date" value={form.effectiveDate} onChange={(e) => setForm({ ...form, effectiveDate: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs font-bold uppercase tracking-wider">تاريخ المعاملة (اختياري)</Label>
              <Input type="date" value={form.transactionDate} onChange={(e) => setForm({ ...form, transactionDate: e.target.value })} className="bg-secondary border-border" />
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label className="text-xs font-bold uppercase tracking-wider">ملاحظات</Label>
              <Input value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} className="bg-secondary border-border" />
            </div>
            <label className="flex items-center gap-2 text-sm cursor-pointer sm:col-span-2 border border-border px-3 py-2">
              <input type="checkbox" checked={form.isRecurring} onChange={(e) => setForm({ ...form, isRecurring: e.target.checked })} /> متكرر شهرياً (يُفعّل في مرحلة لاحقة)
            </label>
          </div>
          <DialogFooter className="gap-2">
            <Button variant="outline" onClick={() => setDialogOpen(false)} disabled={saving}>إلغاء</Button>
            <Button variant="outline" onClick={() => save(false)} disabled={saving} className="font-bold">{saving ? "..." : "حفظ كمسودة"}</Button>
            <Button onClick={() => save(true)} disabled={saving} className="font-bold">{saving ? "جاري الحفظ..." : "حفظ وإرسال للاعتماد"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteTarget} onOpenChange={(o) => { if (!o) setDeleteTarget(null); }}>
        <DialogContent showCloseButton={false}>
          <DialogHeader>
            <DialogTitle>حذف سجل</DialogTitle>
            <DialogDescription>هل أنت متأكد من حذف سجل <span className="font-bold text-foreground">{deleteTarget?.employeeName}</span>؟</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteTarget(null)}>إلغاء</Button>
            <Button onClick={confirmDelete} className="bg-destructive text-white hover:bg-destructive/90">حذف</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
```

> NOTE: confirm `Combobox`'s prop names by reading `src/components/ui/combobox.tsx` — the payroll runs dialog uses `<Combobox value={...} onChange={...} options={[{value,label}]} placeholder=... />`. Match that exact signature; adjust if the component expects different prop names.

- [ ] **Step 3: Write the two route pages**

Create `src/app/(dashboard)/payroll/additions/page.tsx`:

```tsx
"use client";
import { TransactionsPage } from "@/components/payroll/transactions-page";
export default function PayrollAdditionsPage() { return <TransactionsPage kind={1} />; }
```

Create `src/app/(dashboard)/payroll/deductions/page.tsx`:

```tsx
"use client";
import { TransactionsPage } from "@/components/payroll/transactions-page";
export default function PayrollDeductionsPage() { return <TransactionsPage kind={2} />; }
```

- [ ] **Step 4: Add nav links on the payroll landing page**

In `src/app/(dashboard)/payroll/page.tsx`, add a links row inside the `Inner` component's header `<div>` so users can reach the two pages. Add this block immediately after the closing `</div>` of the title block (the `<div>` containing the `<h1>الرواتب</h1>`), before the `{canRun && ...}` button — i.e. restructure the header to include a link row. Concretely, replace the header `<div className="flex items-center justify-between">...</div>` opening so a second row of links renders under it:

Add this `<div>` right after the existing header `flex items-center justify-between` block (after line that closes it, ~line 71):

```tsx
      <div className="flex items-center gap-2">
        <Link href="/payroll/additions" className="text-sm border border-border px-3 py-1.5 hover:bg-card/50 transition-colors">الإضافات</Link>
        <Link href="/payroll/deductions" className="text-sm border border-border px-3 py-1.5 hover:bg-card/50 transition-colors">الاستقطاعات</Link>
      </div>
```

And add the import at the top of the file (it currently imports from `next/navigation`, not `next/link`):

```tsx
import Link from "next/link";
```

- [ ] **Step 5: Type-check + build the frontend**

Run: `npx tsc --noEmit` then `npm run build` (from repo root)
Expected: compiles; the two new routes appear in the build output (`/payroll/additions`, `/payroll/deductions`).

- [ ] **Step 6: Commit**

```bash
git add src/components/payroll/transaction-status-badge.tsx src/components/payroll/transactions-page.tsx "src/app/(dashboard)/payroll/additions/page.tsx" "src/app/(dashboard)/payroll/deductions/page.tsx" "src/app/(dashboard)/payroll/page.tsx"
git commit -m "feat(payroll-ui): additions & deductions pages + nav (2A)"
```

---

### Task 8: Migration apply, full verification, deploy

**Files:** none (operational).

- [ ] **Step 1: Run the full backend test suite**

Run: `dotnet test backend/HR.sln`
Expected: all tests pass, including the new `PayrollTransaction*` tests. Record the pass count.

- [ ] **Step 2: Apply the migration to Azure**

Read the DB password from `~/.hrcloud-db-pass.txt` (per CLAUDE.md) and apply the migration against the production connection string. From `backend/src/HR.Infrastructure` (PowerShell):

```powershell
$pass = (Get-Content "$HOME/.hrcloud-db-pass.txt" -Raw).Trim()
$env:ConnectionStrings__DefaultConnection = "Host=hrcloud-pg-v4xd.postgres.database.azure.com;Port=5432;Database=hrcloud;Username=hradmin;Password=$pass;Ssl Mode=Require;Trust Server Certificate=true"
dotnet ef database update --startup-project ../HR.Api
```
Expected: `PayrollTransactions` migration applies; `engine_payroll_transactions` table created. Then clear the env var: `Remove-Item Env:\ConnectionStrings__DefaultConnection`.

- [ ] **Step 3: Redeploy the API**

Build + zip-deploy the API to `hrcloud-api-v4xd` per the repo's deploy procedure (CLAUDE.md "Backend API — DEPLOYED" note: build the zip via `System.IO.Compression.ZipFile` with `.Replace('\\','/')` on entry names, then `az webapp deploy --type zip`). Confirm Swagger lists the new `api/payroll/transactions` endpoints.

- [ ] **Step 4: Manual end-to-end verification (Swagger or UI)**

Verify the acceptance criteria:
1. `POST /api/payroll/transactions` (kind=2, valid employee + DeductionType id, amount 100, effectiveDate this month) → returns 201 with status `Draft` (0) and `targetPeriodYear/Month` set from the effective date.
2. `POST .../{id}/submit` → status `PendingApproval` (1); `.../{id}/approve` → `Approved` (2).
3. `POST .../{id}/approve` from `Draft` → 400/500 with an illegal-transition error (state machine enforced).
4. `PUT .../{id}` after approval → rejected (edit only in Draft).
5. `GET /api/payroll/transactions?kind=2` → the record appears with resolved `employeeName` + `typeName`.
6. Frontend: `/payroll/deductions` and `/payroll/additions` load, create dialog works, status badges + "سيؤثر على" period render, lifecycle action buttons gate by permission.

- [ ] **Step 5: Update project memory**

Append a memory entry under `C:\Users\yaman\.claude\projects\D--HR-Cloud-main-HR-Cloud-main\memory\` for sub-project 2A (record entity + lifecycle + pages; migration applied to Azure; 2B = engine consumption next), and add the one-line pointer to `MEMORY.md`. Link to `[[financial-calculation-engine]]`.

- [ ] **Step 6: Final commit / branch**

Work is on branch `feat/financial-engine`. Verify `git status` is clean after commits. If a PR is desired, follow `superpowers:finishing-a-development-branch`.

---

## Self-Review

**Spec coverage** (against `2026-06-30-payroll-subproject-2a-transaction-records-design.md`):
- §1 Data model (unified entity, all fields, indexes, validation) → Tasks 2, 4. ✓
- §2 Lifecycle & state machine (states, wired vs deferred transitions, editable-only-in-Draft, Posted immutable) → Tasks 1, 4. ✓
- §3 API surface (all 10 endpoints, permission gating, master-data reuse) → Task 5. ✓
- §4 Frontend (two routes share one component, table/filters/dialog/actions/attachment/impact hint/badges) → Tasks 6, 7. ✓ (Column filters beyond `kind` are supported by the wrapper + service; the UI ships the `kind`-scoped list — additional filter controls are a thin add and acceptable for 2A.)
- §5 Testing (state machine, create/update validation, workflow handlers, tenant scoping, attachment) → Tasks 1, 2, 4. ✓
- §6 Migration & deployment → Tasks 2, 8. ✓
- §7 Cross-increment seams (posting metadata columns, reversal fields, recurring flag, etc. defined but inert) → entity in Task 2 carries all columns; transitions defined in Task 1. ✓

**Placeholder scan:** No "TBD"/"add validation"/"similar to". The one drafting awkwardness (the `Cancelled` dictionary key) is called out with the exact replacement line. Two "confirm signature" notes (BaseApiController helpers, Combobox props) point at named files to read — these are verification guards, not missing content; the assumed signatures match the verbatim examples gathered from those files.

**Type consistency:** `IPayrollTransactionService` method names (`CreateAsync`/`UpdateAsync`/`ListAsync`/`GetAsync`/`SubmitAsync`/`ApproveAsync`/`RejectAsync`/`CancelAsync`/`SetAttachmentAsync`/`DeleteAsync`) are identical across Tasks 3, 4, 5. DTO record `PayrollTransactionDto` field order in Task 3 matches the `Project(...)` constructor call in Task 4. Frontend `PayrollTransaction` interface fields match the backend DTO (camelCased). Enum int values (Kind 1/2; Status 0–7) are consistent across backend enums (Task 1), wrapper types (Task 6), badge styles + action gating (Task 7).

# Payroll Types + Selection Scope + Cutoff — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn the existing `PayrollDefinition`/`PayrollDefinitionVersion` into a configurable, customer-extensible **Payroll Type** with a pluggable Scope Engine for employee selection, calculation settings, cutoff configuration, config versioning (clone/publish/simulate), and per-run population snapshots.

**Architecture:** Enrich the existing immutable-version entities (no parallel "type" entity). Employee selection becomes a provider/strategy pattern: payroll depends only on `IScopeEngine`; each owning module supplies `IScopeDimensionProvider`s discovered via DI assembly scan (mirrors the existing `AddEffectExecutorsFromAssembly` pattern). Configurable Category/Export-Format are new master-data `ObjectType`s. Every run freezes its resolved population into `PayrollRunPopulation`.

**Tech Stack:** .NET 8, EF Core 8 (Npgsql / PostgreSQL), xUnit + EF Core InMemory provider, Next.js (App Router) + React, TypeScript, RTL/Arabic UI, `AccessGuard` permission gating.

## Global Constraints

- **No mock data, no placeholders** — every screen and endpoint backed by real persisted data.
- **No duplicate fields** — reuse canonical fields (master-data items, existing version columns). New configurable catalogs are new `MasterDataObjectType`s, never new tables.
- **Immutability** — config edits publish a new `PayrollDefinitionVersion`; runs pin the version + `RuleSetVersionId`; runs snapshot their resolved population so org changes never alter historical runs.
- **Payroll never references another module's schema directly** — only `IScopeEngine` and the exporter handler registry abstractions.
- **Strongly-typed columns for hot/queryable fields; JSON for flexible/advanced settings.**
- **Backend entities live in `HR.Domain`; application interfaces in `HR.Application`; implementations in `HR.Infrastructure`; HTTP surface in `HR.Modules`.** (Preserves the no-circular-deps structure.)
- **All money rounding `MidpointRounding.AwayFromZero`, 2 dp** (matches `RuleEngineCore`).
- **DB dates are UTC** (Postgres `timestamptz`); normalize `DateTime` to `DateTimeKind.Utc` before persisting (see commit a8f5e85).
- Permissions: reads `Payroll.View`; config mutations new `Payroll.Configure`.

---

## File Structure

**Backend — create:**
- `backend/src/HR.Domain/Engines/Finance/Entities/PayrollRunPopulation.cs` — frozen per-run resolved employee snapshot.
- `backend/src/HR.Application/Engines/Scope/ScopeContracts.cs` — `ScopeDimensionInfo`, `ScopeValueSource`, `SelectionScope`, `ScopeCriterion`, `ScopeResolution`, `ScopeExclusion`.
- `backend/src/HR.Application/Engines/Scope/IScopeDimensionProvider.cs` — provider + base-population interfaces.
- `backend/src/HR.Application/Engines/Scope/IScopeEngine.cs` — the engine abstraction.
- `backend/src/HR.Application/Engines/Scope/ScopeServiceCollectionExtensions.cs` — `AddScopeProvidersFromAssembly`.
- `backend/src/HR.Infrastructure/Engines/Scope/ScopeEngine.cs` — aggregates providers, resolves.
- `backend/src/HR.Infrastructure/Engines/Scope/StaticDisabledDimensions.cs` — registry of not-yet-backed dimensions.
- `backend/src/HR.Modules/Employees/Scope/EmployeeScopeProviders.cs` — 8 backed providers + base population.
- `backend/src/HR.Application/Engines/Finance/IPayrollTypeService.cs` — type/version CRUD + clone/publish/simulate.
- `backend/src/HR.Infrastructure/Engines/Finance/PayrollTypeService.cs` — impl.
- `backend/src/HR.Modules/Payroll/DTOs/PayrollTypeDtos.cs` — request/response DTOs.
- `backend/tests/HR.Domain.Finance.Tests/ScopeEngineTests.cs`, `SelectionScopeJsonTests.cs`, `PayrollTypeServiceTests.cs`, `DayBasisProrationTests.cs`.

**Backend — modify:**
- `backend/src/HR.Domain/Enums/FinanceEnums.cs` — add `DayBasis`.
- `backend/src/HR.Domain/Engines/Finance/Entities/PayrollDefinition.cs` — `CategoryId`; new version columns.
- `backend/src/HR.Domain/Engines/MasterData/MasterDataObjectType.cs` — `PayrollTypeCategory`, `PayrollExportFormat`.
- `backend/src/HR.Infrastructure/Persistence/Configurations/Engines/*` — EF config for new columns + `PayrollRunPopulation`.
- `backend/src/HR.Infrastructure/Persistence/MasterDataDefaults.cs` — seed catalog rows.
- `backend/src/HR.Infrastructure/Engines/Finance/PayrollFactProvider.cs` — resolve via `IScopeEngine`, `DayBasis` proration, optional population restriction.
- `backend/src/HR.Infrastructure/Engines/Finance/PayrollComputation.cs` — thread optional restrict-to-ids.
- `backend/src/HR.Infrastructure/Engines/Finance/PayrollRunEngine.cs` — freeze population in `CreateAsync`; pass frozen ids in `CalculateAsync`.
- `backend/src/HR.Infrastructure/Engines/Finance/StandardPayrollSeeder.cs` — stamp MONTHLY type defaults.
- `backend/src/HR.Infrastructure/DependencyInjection.cs` — register `ScopeEngine`, `PayrollTypeService`, scope providers (Infrastructure assembly).
- `backend/src/HR.Modules/Employees/DependencyInjection/DependencyInjection.cs` — `AddScopeProvidersFromAssembly` for Employees.
- `backend/src/HR.Modules/Payroll/Controllers/PayrollController.cs` — type/version/scope endpoints.
- The permission catalog source (where `Payroll.View/Run/Approve/Lock` are declared) — add `Payroll.Configure`.

**Frontend — create:**
- `src/lib/api/payroll-types.ts` — typed client + DTO types.
- `src/app/(dashboard)/settings/payroll/types/page.tsx` — type list.
- `src/app/(dashboard)/settings/payroll/types/[id]/page.tsx` — type detail + version editor.
- `src/components/payroll/scope-builder.tsx` — include/exclude dimension builder with live count.

---

## Task 1: Schema — DayBasis enum, version columns, CategoryId, PayrollRunPopulation, migration

**Files:**
- Modify: `backend/src/HR.Domain/Enums/FinanceEnums.cs`
- Modify: `backend/src/HR.Domain/Engines/Finance/Entities/PayrollDefinition.cs`
- Create: `backend/src/HR.Domain/Engines/Finance/Entities/PayrollRunPopulation.cs`
- Modify: EF config under `backend/src/HR.Infrastructure/Persistence/Configurations/Engines/` (the file configuring finance entities — locate via grep for `engine_payroll_definition_versions`)
- Create: migration `backend/src/HR.Infrastructure/Migrations/<timestamp>_PayrollTypesAndScope.cs` (generated)

**Interfaces:**
- Produces: `DayBasis { CalendarMonth=1, Fixed30=2, WorkingDays=3 }`; `PayrollDefinition.CategoryId : Guid?`; on `PayrollDefinitionVersion`: `CutoffDay:int`, `DayBasis:DayBasis`, `ClosingDate:DateTime?`, `PaymentDate:DateTime?`, `CarryToNextPeriod:bool`, `DefaultExportFormatId:Guid?`, `EffectiveFrom:DateTime?`, `EffectiveTo:DateTime?`, `IsSimulation:bool`, `SelectionScopeJson:string?`, `CalcSettingsJson:string?`, `PaymentMethodScopeJson:string?`; entity `PayrollRunPopulation` (table `engine_payroll_run_population`).

- [ ] **Step 1: Add the `DayBasis` enum**

In `backend/src/HR.Domain/Enums/FinanceEnums.cs`, add:

```csharp
/// <summary>How a month's daily wage is prorated.</summary>
public enum DayBasis
{
    CalendarMonth = 1, // basic / actual days in the month
    Fixed30 = 2,       // basic / 30
    WorkingDays = 3,   // basic / working days in the month
}
```

- [ ] **Step 2: Add `CategoryId` + the new version columns**

In `PayrollDefinition.cs`, add to `PayrollDefinition`:

```csharp
    /// <summary>Master-data PayrollTypeCategory item id (Regular / Mudad / Cash / Bonus / EOS / Off-cycle).</summary>
    public Guid? CategoryId { get; set; }
```

Add to `PayrollDefinitionVersion` (after `CycleConfigJson`; `CycleConfigJson` is now superseded by the typed columns below and left unused for back-compat):

```csharp
    // --- Payroll Type configuration (sub-project 1). Typed columns for hot/queryable fields. ---
    public int CutoffDay { get; set; } = 27;
    public DayBasis DayBasis { get; set; } = DayBasis.CalendarMonth;
    public DateTime? ClosingDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public bool CarryToNextPeriod { get; set; } = true;
    public Guid? DefaultExportFormatId { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsSimulation { get; set; }

    /// <summary>Rich employee selection (include/exclude across registered scope dimensions).</summary>
    public string? SelectionScopeJson { get; set; }
    /// <summary>Calculation toggles (include/exclude allowances/additions/etc.). DayBasis is the typed column.</summary>
    public string? CalcSettingsJson { get; set; }
    /// <summary>Allowed master-data PaymentMethod ids for this type.</summary>
    public string? PaymentMethodScopeJson { get; set; }
```

- [ ] **Step 3: Create the `PayrollRunPopulation` entity**

Create `PayrollRunPopulation.cs`:

```csharp
using HR.Domain.Common;

namespace HR.Domain.Engines.Finance.Entities;

/// <summary>A frozen snapshot of one employee resolved into a payroll run at creation time. Future
/// organizational changes never alter a historical run because the run reads this snapshot, not live data.</summary>
public class PayrollRunPopulation : TenantEntity
{
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? JobTitleId { get; set; }
    public Guid? PaymentMethodId { get; set; }

    public bool IsIncluded { get; set; } = true;
    /// <summary>Null when included. Sub-project 1 sets "ExcludedByScope"; sub-project 3 adds validity reasons.</summary>
    public string? ExclusionReasonCode { get; set; }
}
```

- [ ] **Step 4: Register the entity + EF config**

In `ApplicationDbContext` (grep for `DbSet<PayrollPayslip>`), add:

```csharp
    public DbSet<PayrollRunPopulation> PayrollRunPopulations => Set<PayrollRunPopulation>();
```

In the finance EF configuration file (grep `ToTable("engine_payroll_run_items")` to find it), add a configuration block:

```csharp
        builder.Entity<PayrollRunPopulation>(e =>
        {
            e.ToTable("engine_payroll_run_population");
            e.HasIndex(x => new { x.TenantId, x.PayrollRunId });
            e.Property(x => x.EmployeeNumber).HasMaxLength(64);
            e.Property(x => x.EmployeeName).HasMaxLength(256);
            e.Property(x => x.ExclusionReasonCode).HasMaxLength(64);
        });
```

(Match the exact registration idiom used by the neighbouring `engine_payroll_run_items` config — fluent block vs `IEntityTypeConfiguration` class.)

- [ ] **Step 5: Generate the migration**

Run (from `backend`):

```bash
dotnet ef migrations add PayrollTypesAndScope -p src/HR.Infrastructure -s src/HR.Api
```

Expected: a new `<timestamp>_PayrollTypesAndScope.cs` adding `CategoryId` to `engine_payroll_definitions`, the new columns to `engine_payroll_definition_versions`, and the `engine_payroll_run_population` table.

- [ ] **Step 6: Add the back-compat backfill to the migration `Up`**

At the end of the generated `Up(MigrationBuilder migrationBuilder)`, append SQL copying the legacy filter into the new scope JSON (departments + employee ids → include criteria), so the seeded `MONTHLY` type and existing runs keep resolving:

```csharp
            migrationBuilder.Sql(@"
UPDATE engine_payroll_definition_versions
SET ""SelectionScopeJson"" = jsonb_build_object(
        'mode', 'Criteria',
        'include', CASE
            WHEN ""EmployeeFilterJson"" IS NOT NULL
                 AND (""EmployeeFilterJson""::jsonb ? 'departmentIds')
            THEN jsonb_build_array(jsonb_build_object(
                'dimension', 'Department',
                'valueIds', (""EmployeeFilterJson""::jsonb -> 'departmentIds')))
            ELSE '[]'::jsonb END,
        'exclude', '[]'::jsonb,
        'includeEmployeeIds', COALESCE(""EmployeeFilterJson""::jsonb -> 'employeeIds', '[]'::jsonb),
        'excludeEmployeeIds', '[]'::jsonb
    )::text
WHERE ""SelectionScopeJson"" IS NULL;");
```

> If `EmployeeFilterJson` had no departments, `mode` should still allow runs; the `ScopeEngine` treats empty includes under `Criteria` as "no criteria matched" — so also set any all-null legacy rows to `mode='All'`. Add:

```csharp
            migrationBuilder.Sql(@"
UPDATE engine_payroll_definition_versions
SET ""SelectionScopeJson"" = jsonb_set(""SelectionScopeJson""::jsonb, '{mode}', '""All""')::text
WHERE (""SelectionScopeJson""::jsonb -> 'include') = '[]'::jsonb
  AND (""SelectionScopeJson""::jsonb -> 'includeEmployeeIds') = '[]'::jsonb;");
```

- [ ] **Step 7: Build to verify schema compiles + migration is valid**

Run:

```bash
dotnet build backend/src/HR.Infrastructure/HR.Infrastructure.csproj
dotnet ef migrations script --idempotent -p backend/src/HR.Infrastructure -s backend/src/HR.Api -o /tmp/payroll-scope.sql
```

Expected: build succeeds; the script contains `engine_payroll_run_population` and the new columns.

- [ ] **Step 8: Commit**

```bash
git add backend/src/HR.Domain backend/src/HR.Infrastructure
git commit -m "feat(payroll): payroll-type config columns, run-population snapshot, migration"
```

---

## Task 2: Configurable catalogs — PayrollTypeCategory + PayrollExportFormat

**Files:**
- Modify: `backend/src/HR.Domain/Engines/MasterData/MasterDataObjectType.cs`
- Modify: `backend/src/HR.Infrastructure/Persistence/MasterDataDefaults.cs`

**Interfaces:**
- Produces: `MasterDataObjectType.PayrollTypeCategory = "PayrollTypeCategory"`, `MasterDataObjectType.PayrollExportFormat = "PayrollExportFormat"`; seeded default rows for both.

- [ ] **Step 1: Add the two object types**

In `MasterDataObjectType.cs`, add two constants and append them to `All`:

```csharp
    public const string PayrollTypeCategory = "PayrollTypeCategory";
    public const string PayrollExportFormat = "PayrollExportFormat";
```

```csharp
        // ...existing entries...
        RecruitmentSource, CandidateStage, Tag, Skill, Bank, Nationality,
        PayrollTypeCategory, PayrollExportFormat
```

- [ ] **Step 2: Seed default rows**

Open `MasterDataDefaults.cs`, read the existing `MasterDataDefault` record shape and the list it appends to (grep for an existing entry like `PaymentMethod`). Following that exact shape, add rows. Categories:

```csharp
        // Payroll type categories (extensible). MetadataJson carries default export format + payment scope.
        new(MasterDataObjectType.PayrollTypeCategory, "REGULAR",  "Regular Monthly", "الرواتب الشهرية",   metadataJson: "{\"defaultExportFormatCode\":\"PDF\"}"),
        new(MasterDataObjectType.PayrollTypeCategory, "MUDAD",    "Mudad Payroll",   "رواتب مدد",          metadataJson: "{\"defaultExportFormatCode\":\"MUDAD\"}"),
        new(MasterDataObjectType.PayrollTypeCategory, "CASH",     "Cash Payroll",    "رواتب نقدية",        metadataJson: "{\"defaultExportFormatCode\":\"CASH\"}"),
        new(MasterDataObjectType.PayrollTypeCategory, "BONUS",    "Bonus Payroll",   "مسير المكافآت",      metadataJson: "{\"defaultExportFormatCode\":\"PDF\"}"),
        new(MasterDataObjectType.PayrollTypeCategory, "EOS",      "End of Service",  "مسير نهاية الخدمة",  metadataJson: "{\"defaultExportFormatCode\":\"PDF\"}"),
        new(MasterDataObjectType.PayrollTypeCategory, "OFFCYCLE", "Off-cycle",       "مسير استثنائي",      metadataJson: "{\"defaultExportFormatCode\":\"PDF\"}"),
        // Export formats. MetadataJson.handlerKey maps to a code-registered exporter (sub-project 5).
        new(MasterDataObjectType.PayrollExportFormat, "PDF",   "PDF",            "PDF",            metadataJson: "{\"handlerKey\":\"pdf\"}"),
        new(MasterDataObjectType.PayrollExportFormat, "EXCEL", "Excel",          "إكسل",           metadataJson: "{\"handlerKey\":\"excel\"}"),
        new(MasterDataObjectType.PayrollExportFormat, "CSV",   "CSV",            "CSV",            metadataJson: "{\"handlerKey\":\"csv\"}"),
        new(MasterDataObjectType.PayrollExportFormat, "TXT",   "Text",           "نص",             metadataJson: "{\"handlerKey\":\"txt\"}"),
        new(MasterDataObjectType.PayrollExportFormat, "BANK",  "Bank Transfer",  "تحويل بنكي",     metadataJson: "{\"handlerKey\":\"bank\"}"),
        new(MasterDataObjectType.PayrollExportFormat, "MUDAD", "Mudad File",     "ملف مدد",        metadataJson: "{\"handlerKey\":\"mudad\"}"),
        new(MasterDataObjectType.PayrollExportFormat, "CASH",  "Cash Sheet",     "كشف نقدي",       metadataJson: "{\"handlerKey\":\"cash\"}"),
```

> Adjust the constructor argument names/positions to match the real `MasterDataDefault` record (it may use positional `NameEn, NameAr` and a named/optional `metadataJson`). Do not invent parameters that don't exist — read the record first.

- [ ] **Step 3: Build**

```bash
dotnet build backend/src/HR.Infrastructure/HR.Infrastructure.csproj
```

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add backend/src/HR.Domain backend/src/HR.Infrastructure
git commit -m "feat(payroll): configurable PayrollTypeCategory + PayrollExportFormat catalogs"
```

---

## Task 3: Scope abstractions (HR.Application)

**Files:**
- Create: `backend/src/HR.Application/Engines/Scope/ScopeContracts.cs`
- Create: `backend/src/HR.Application/Engines/Scope/IScopeDimensionProvider.cs`
- Create: `backend/src/HR.Application/Engines/Scope/IScopeEngine.cs`
- Create: `backend/src/HR.Application/Engines/Scope/ScopeServiceCollectionExtensions.cs`

**Interfaces:**
- Produces: `SelectionScope`, `ScopeCriterion`, `ScopeResolution`, `ScopeExclusion`, `ScopeDimensionInfo`, `ScopeValueSource`, `IScopeDimensionProvider`, `IBasePopulationProvider`, `IScopeEngine`, `AddScopeProvidersFromAssembly`. These are consumed by Tasks 4, 5, 6, 8, 9, 11.

- [ ] **Step 1: Write the contracts**

`ScopeContracts.cs`:

```csharp
namespace HR.Application.Engines.Scope;

/// <summary>Where the UI fetches selectable values for a dimension.</summary>
public enum ScopeValueSourceKind { MasterData, StaticEnum, Custom }

public sealed record ScopeValueSource(ScopeValueSourceKind Kind, string? Reference);
// Reference: master-data object-type slug (MasterData), or an opaque key (StaticEnum/Custom).

/// <summary>Describes a selection dimension to the scope builder UI and the engine.</summary>
public sealed record ScopeDimensionInfo(
    string Key,
    string NameEn,
    string NameAr,
    ScopeValueSource ValueSource,
    bool IsAvailable,
    string? UnavailableNote);

public sealed record ScopeCriterion(string Dimension, IReadOnlyList<Guid> ValueIds);

/// <summary>Deserialized from PayrollDefinitionVersion.SelectionScopeJson.</summary>
public sealed record SelectionScope(
    string Mode,                                   // "All" | "Criteria"
    IReadOnlyList<ScopeCriterion> Include,
    IReadOnlyList<ScopeCriterion> Exclude,
    IReadOnlyList<Guid> IncludeEmployeeIds,
    IReadOnlyList<Guid> ExcludeEmployeeIds)
{
    public static SelectionScope All() =>
        new("All", Array.Empty<ScopeCriterion>(), Array.Empty<ScopeCriterion>(),
            Array.Empty<Guid>(), Array.Empty<Guid>());
}

public sealed record ScopeExclusion(Guid EmployeeId, string DimensionKey);

public sealed record ScopeResolution(
    IReadOnlyCollection<Guid> IncludedEmployeeIds,
    IReadOnlyCollection<ScopeExclusion> ExcludedByScope,
    IReadOnlyCollection<string> Warnings);
```

- [ ] **Step 2: Write the provider interfaces**

`IScopeDimensionProvider.cs`:

```csharp
namespace HR.Application.Engines.Scope;

/// <summary>Implemented by the module that OWNS a dimension's data. Payroll never implements these.</summary>
public interface IScopeDimensionProvider
{
    string DimensionKey { get; }
    ScopeDimensionInfo Info { get; }
    Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> valueIds, CancellationToken ct);
}

/// <summary>Owns the "all active employees" base population (mode = All).</summary>
public interface IBasePopulationProvider
{
    Task<ISet<Guid>> ResolveAllAsync(CancellationToken ct);
}
```

- [ ] **Step 3: Write the engine interface**

`IScopeEngine.cs`:

```csharp
namespace HR.Application.Engines.Scope;

public interface IScopeEngine
{
    /// <summary>Every dimension known to the system — available and disabled-with-note — for the UI.</summary>
    IReadOnlyList<ScopeDimensionInfo> Dimensions();

    Task<ScopeResolution> ResolveAsync(SelectionScope scope, CancellationToken ct);
}
```

- [ ] **Step 4: Write the assembly-scan registrar**

`ScopeServiceCollectionExtensions.cs` (mirrors `AddEffectExecutorsFromAssembly`):

```csharp
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Application.Engines.Scope;

public static class ScopeServiceCollectionExtensions
{
    /// <summary>Registers every non-abstract IScopeDimensionProvider / IBasePopulationProvider in the
    /// assembly as scoped. Each owning module calls this for its own assembly; the ScopeEngine then
    /// discovers new dimensions automatically — payroll never changes.</summary>
    public static IServiceCollection AddScopeProvidersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        foreach (var t in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false }))
        {
            if (typeof(IScopeDimensionProvider).IsAssignableFrom(t))
                services.AddScoped(typeof(IScopeDimensionProvider), t);
            if (typeof(IBasePopulationProvider).IsAssignableFrom(t))
                services.AddScoped(typeof(IBasePopulationProvider), t);
        }
        return services;
    }
}
```

- [ ] **Step 5: Build + commit**

```bash
dotnet build backend/src/HR.Application/HR.Application.csproj
git add backend/src/HR.Application/Engines/Scope
git commit -m "feat(scope): pluggable scope-engine abstractions"
```

---

## Task 4: SelectionScope JSON parse/serialize (+ tests)

**Files:**
- Create: `backend/src/HR.Application/Engines/Scope/SelectionScopeJson.cs`
- Test: `backend/tests/HR.Domain.Finance.Tests/SelectionScopeJsonTests.cs`

**Interfaces:**
- Consumes: `SelectionScope`, `ScopeCriterion` (Task 3).
- Produces: `SelectionScopeJson.Parse(string?) : SelectionScope`, `SelectionScopeJson.Serialize(SelectionScope) : string`. Consumed by Tasks 8, 9, 11.

- [ ] **Step 1: Write the failing test**

`SelectionScopeJsonTests.cs`:

```csharp
using HR.Application.Engines.Scope;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class SelectionScopeJsonTests
{
    [Fact]
    public void Parse_null_returns_mode_All()
    {
        var s = SelectionScopeJson.Parse(null);
        Assert.Equal("All", s.Mode);
        Assert.Empty(s.Include);
    }

    [Fact]
    public void Parse_reads_include_exclude_and_employee_ids()
    {
        var dep = Guid.NewGuid();
        var emp = Guid.NewGuid();
        var json = $@"{{""mode"":""Criteria"",
            ""include"":[{{""dimension"":""Department"",""valueIds"":[""{dep}""]}}],
            ""exclude"":[{{""dimension"":""Status"",""valueIds"":[],""employeeIds"":[]}}],
            ""includeEmployeeIds"":[""{emp}""],""excludeEmployeeIds"":[]}}";
        var s = SelectionScopeJson.Parse(json);
        Assert.Equal("Criteria", s.Mode);
        Assert.Equal("Department", s.Include[0].Dimension);
        Assert.Equal(dep, s.Include[0].ValueIds[0]);
        Assert.Equal(emp, s.IncludeEmployeeIds[0]);
    }

    [Fact]
    public void Serialize_then_Parse_roundtrips()
    {
        var dep = Guid.NewGuid();
        var original = new SelectionScope("Criteria",
            new[] { new ScopeCriterion("Department", new[] { dep }) },
            Array.Empty<ScopeCriterion>(), Array.Empty<Guid>(), Array.Empty<Guid>());
        var back = SelectionScopeJson.Parse(SelectionScopeJson.Serialize(original));
        Assert.Equal(dep, back.Include[0].ValueIds[0]);
    }

    [Fact]
    public void Parse_malformed_returns_mode_All()
    {
        Assert.Equal("All", SelectionScopeJson.Parse("{ not json").Mode);
    }
}
```

- [ ] **Step 2: Run to verify it fails**

```bash
dotnet test backend/tests/HR.Domain.Finance.Tests --filter SelectionScopeJsonTests
```

Expected: FAIL — `SelectionScopeJson` does not exist.

- [ ] **Step 3: Implement**

`SelectionScopeJson.cs`:

```csharp
using System.Text.Json;

namespace HR.Application.Engines.Scope;

/// <summary>Tolerant parse/serialize for PayrollDefinitionVersion.SelectionScopeJson. Any malformed or
/// missing config degrades to mode = All (never throws), so a bad config can never empty a payroll silently.</summary>
public static class SelectionScopeJson
{
    private static readonly JsonSerializerOptions Opts = new() { PropertyNameCaseInsensitive = true };

    public static SelectionScope Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return SelectionScope.All();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return SelectionScope.All();

            var mode = root.TryGetProperty("mode", out var m) && m.ValueKind == JsonValueKind.String
                ? m.GetString()! : "All";
            return new SelectionScope(
                mode,
                ReadCriteria(root, "include"),
                ReadCriteria(root, "exclude"),
                ReadGuids(root, "includeEmployeeIds"),
                ReadGuids(root, "excludeEmployeeIds"));
        }
        catch (JsonException) { return SelectionScope.All(); }
    }

    public static string Serialize(SelectionScope scope) => JsonSerializer.Serialize(new
    {
        mode = scope.Mode,
        include = scope.Include.Select(c => new { dimension = c.Dimension, valueIds = c.ValueIds }),
        exclude = scope.Exclude.Select(c => new { dimension = c.Dimension, valueIds = c.ValueIds }),
        includeEmployeeIds = scope.IncludeEmployeeIds,
        excludeEmployeeIds = scope.ExcludeEmployeeIds,
    }, Opts);

    private static List<ScopeCriterion> ReadCriteria(JsonElement root, string prop)
    {
        var list = new List<ScopeCriterion>();
        if (root.TryGetProperty(prop, out var arr) && arr.ValueKind == JsonValueKind.Array)
            foreach (var el in arr.EnumerateArray())
            {
                if (el.ValueKind != JsonValueKind.Object) continue;
                var dim = el.TryGetProperty("dimension", out var d) ? d.GetString() : null;
                if (string.IsNullOrWhiteSpace(dim)) continue;
                list.Add(new ScopeCriterion(dim!, ReadGuidArray(el, "valueIds")));
            }
        return list;
    }

    private static List<Guid> ReadGuids(JsonElement root, string prop) => ReadGuidArray(root, prop);

    private static List<Guid> ReadGuidArray(JsonElement obj, string prop)
    {
        var ids = new List<Guid>();
        if (obj.TryGetProperty(prop, out var arr) && arr.ValueKind == JsonValueKind.Array)
            foreach (var el in arr.EnumerateArray())
                if (Guid.TryParse(el.GetString(), out var g)) ids.Add(g);
        return ids;
    }
}
```

- [ ] **Step 4: Run to verify pass**

```bash
dotnet test backend/tests/HR.Domain.Finance.Tests --filter SelectionScopeJsonTests
```

Expected: PASS (4 tests).

- [ ] **Step 5: Commit**

```bash
git add backend/src/HR.Application/Engines/Scope/SelectionScopeJson.cs backend/tests/HR.Domain.Finance.Tests/SelectionScopeJsonTests.cs
git commit -m "feat(scope): tolerant SelectionScope JSON parse/serialize"
```

---

## Task 5: ScopeEngine implementation (+ tests)

**Files:**
- Create: `backend/src/HR.Infrastructure/Engines/Scope/StaticDisabledDimensions.cs`
- Create: `backend/src/HR.Infrastructure/Engines/Scope/ScopeEngine.cs`
- Test: `backend/tests/HR.Domain.Finance.Tests/ScopeEngineTests.cs`

**Interfaces:**
- Consumes: `IScopeEngine`, `IScopeDimensionProvider`, `IBasePopulationProvider`, `SelectionScope`, `ScopeResolution`, `ScopeDimensionInfo` (Task 3).
- Produces: `ScopeEngine` (registered as `IScopeEngine` in Task 13); `StaticDisabledDimensions.All` list. Consumed by Tasks 8, 9, 11.

- [ ] **Step 1: Write the failing test**

The test uses fake providers (no DB) to lock the resolution algebra. `ScopeEngineTests.cs`:

```csharp
using HR.Application.Engines.Scope;
using HR.Infrastructure.Engines.Scope;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class ScopeEngineTests
{
    private static readonly Guid A = Guid.NewGuid(), B = Guid.NewGuid(), C = Guid.NewGuid();

    private sealed class FakeBase : IBasePopulationProvider
    {
        private readonly Guid[] _all;
        public FakeBase(params Guid[] all) => _all = all;
        public Task<ISet<Guid>> ResolveAllAsync(CancellationToken ct) =>
            Task.FromResult<ISet<Guid>>(new HashSet<Guid>(_all));
    }

    private sealed class FakeDim : IScopeDimensionProvider
    {
        private readonly Dictionary<Guid, Guid[]> _map;
        public FakeDim(string key, Dictionary<Guid, Guid[]> map) { DimensionKey = key; _map = map; }
        public string DimensionKey { get; }
        public ScopeDimensionInfo Info => new(DimensionKey, DimensionKey, DimensionKey,
            new ScopeValueSource(ScopeValueSourceKind.Custom, null), true, null);
        public Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> valueIds, CancellationToken ct)
        {
            var set = new HashSet<Guid>();
            foreach (var v in valueIds) if (_map.TryGetValue(v, out var emps)) set.UnionWith(emps);
            return Task.FromResult<ISet<Guid>>(set);
        }
    }

    private static ScopeEngine Engine(IBasePopulationProvider basePop, params IScopeDimensionProvider[] dims) =>
        new(dims, basePop);

    [Fact]
    public async Task Mode_All_returns_base_population()
    {
        var e = Engine(new FakeBase(A, B, C));
        var r = await e.ResolveAsync(SelectionScope.All(), default);
        Assert.Equal(new[] { A, B, C }.OrderBy(x => x), r.IncludedEmployeeIds.OrderBy(x => x));
    }

    [Fact]
    public async Task Within_dimension_is_OR_across_dimensions_is_AND()
    {
        var depSales = Guid.NewGuid(); var depOps = Guid.NewGuid(); var branchHQ = Guid.NewGuid();
        var dep = new FakeDim("Department", new() { [depSales] = new[] { A, B }, [depOps] = new[] { C } });
        var br  = new FakeDim("Branch", new() { [branchHQ] = new[] { B, C } });
        var e = Engine(new FakeBase(A, B, C), dep, br);
        var scope = new SelectionScope("Criteria",
            new[] { new ScopeCriterion("Department", new[] { depSales, depOps }),  // A,B,C
                    new ScopeCriterion("Branch", new[] { branchHQ }) },            // B,C
            Array.Empty<ScopeCriterion>(), Array.Empty<Guid>(), Array.Empty<Guid>());
        var r = await e.ResolveAsync(scope, default);
        Assert.Equal(new[] { B, C }.OrderBy(x => x), r.IncludedEmployeeIds.OrderBy(x => x)); // intersection
    }

    [Fact]
    public async Task Exclude_wins()
    {
        var depSales = Guid.NewGuid();
        var dep = new FakeDim("Department", new() { [depSales] = new[] { A, B, C } });
        var e = Engine(new FakeBase(A, B, C), dep);
        var scope = new SelectionScope("Criteria",
            new[] { new ScopeCriterion("Department", new[] { depSales }) },
            Array.Empty<ScopeCriterion>(), Array.Empty<Guid>(), new[] { B });
        var r = await e.ResolveAsync(scope, default);
        Assert.DoesNotContain(B, r.IncludedEmployeeIds);
        Assert.Contains(B, r.ExcludedByScope.Select(x => x.EmployeeId));
    }

    [Fact]
    public async Task Unavailable_dimension_is_skipped_with_warning()
    {
        var e = Engine(new FakeBase(A, B));
        var scope = new SelectionScope("Criteria",
            new[] { new ScopeCriterion("CostCenter", new[] { Guid.NewGuid() }) },
            Array.Empty<ScopeCriterion>(), Array.Empty<Guid>(), Array.Empty<Guid>());
        var r = await e.ResolveAsync(scope, default);
        Assert.NotEmpty(r.Warnings);            // CostCenter has no provider
        Assert.Empty(r.IncludedEmployeeIds);    // no include matched
    }

    [Fact]
    public void Dimensions_includes_disabled_ones_with_notes()
    {
        var e = Engine(new FakeBase(), new FakeDim("Department", new()));
        var dims = e.Dimensions();
        Assert.Contains(dims, d => d.Key == "Department" && d.IsAvailable);
        Assert.Contains(dims, d => d.Key == "CostCenter" && !d.IsAvailable && d.UnavailableNote != null);
    }
}
```

- [ ] **Step 2: Run to verify it fails**

```bash
dotnet test backend/tests/HR.Domain.Finance.Tests --filter ScopeEngineTests
```

Expected: FAIL — `ScopeEngine` / `StaticDisabledDimensions` do not exist. (Note: `HR.Domain.Finance.Tests` must reference `HR.Infrastructure`; it already does for other infra-touching tests — if not, add the project reference.)

- [ ] **Step 3: Implement the disabled-dimension registry**

`StaticDisabledDimensions.cs`:

```csharp
using HR.Application.Engines.Scope;

namespace HR.Infrastructure.Engines.Scope;

/// <summary>Dimensions the product intends to support but whose owning module has not yet shipped a
/// provider. Surfaced (disabled) in the scope builder so the model is forward-compatible.</summary>
public static class StaticDisabledDimensions
{
    public static readonly IReadOnlyList<ScopeDimensionInfo> All = new[]
    {
        Disabled("Tag", "Tags", "الوسوم", "tags"),
        Disabled("CostCenter", "Cost Center", "مركز التكلفة", "cost-centers"),
        Disabled("Grade", "Grade", "الدرجة الوظيفية", "grades"),
        Disabled("Shift", "Shift", "الوردية", "shift-types"),
        Disabled("Project", "Project", "المشروع", null),
        Disabled("BusinessUnit", "Business Unit", "وحدة الأعمال", null),
        Disabled("Company", "Company", "الشركة", null),
    };

    private static ScopeDimensionInfo Disabled(string key, string en, string ar, string? slug) =>
        new(key, en, ar,
            new ScopeValueSource(slug is null ? ScopeValueSourceKind.Custom : ScopeValueSourceKind.MasterData, slug),
            IsAvailable: false,
            UnavailableNote: "Backing provider not yet available");
}
```

- [ ] **Step 4: Implement the engine**

`ScopeEngine.cs`:

```csharp
using HR.Application.Engines.Scope;

namespace HR.Infrastructure.Engines.Scope;

/// <summary>Aggregates every registered dimension provider into a Key→provider map (last-wins override,
/// same as the completion EffectExecutorRegistry) and resolves a SelectionScope into an employee set.
/// Payroll depends only on IScopeEngine; it never touches Employee columns.</summary>
public sealed class ScopeEngine : IScopeEngine
{
    private readonly Dictionary<string, IScopeDimensionProvider> _providers;
    private readonly IBasePopulationProvider _basePopulation;

    public ScopeEngine(IEnumerable<IScopeDimensionProvider> providers, IBasePopulationProvider basePopulation)
    {
        _providers = new(StringComparer.OrdinalIgnoreCase);
        foreach (var p in providers) _providers[p.DimensionKey] = p; // last wins
        _basePopulation = basePopulation;
    }

    public IReadOnlyList<ScopeDimensionInfo> Dimensions()
    {
        var available = _providers.Values.Select(p => p.Info).ToList();
        var keys = available.Select(i => i.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var disabled = StaticDisabledDimensions.All.Where(d => !keys.Contains(d.Key));
        return available.Concat(disabled).OrderByDescending(d => d.IsAvailable).ThenBy(d => d.NameEn).ToList();
    }

    public async Task<ScopeResolution> ResolveAsync(SelectionScope scope, CancellationToken ct)
    {
        var warnings = new List<string>();

        // 1. Base set.
        ISet<Guid> set = string.Equals(scope.Mode, "All", StringComparison.OrdinalIgnoreCase)
            ? await _basePopulation.ResolveAllAsync(ct)
            : new HashSet<Guid>();

        // 2. Includes: OR within a dimension, AND across dimensions.
        if (!string.Equals(scope.Mode, "All", StringComparison.OrdinalIgnoreCase))
        {
            var first = true;
            foreach (var c in scope.Include)
            {
                if (!_providers.TryGetValue(c.Dimension, out var provider))
                { warnings.Add($"Dimension '{c.Dimension}' is not available and was skipped."); continue; }
                var matched = await provider.ResolveEmployeesAsync(c.ValueIds, ct);
                if (first) { set = matched; first = false; }
                else set.IntersectWith(matched);
            }
        }

        // 3. Explicit include ids unioned in.
        foreach (var id in scope.IncludeEmployeeIds) set.Add(id);

        // 4. Excludes (exclude always wins).
        var excluded = new List<ScopeExclusion>();
        foreach (var c in scope.Exclude)
        {
            if (!_providers.TryGetValue(c.Dimension, out var provider))
            { warnings.Add($"Dimension '{c.Dimension}' is not available and was skipped."); continue; }
            foreach (var id in await provider.ResolveEmployeesAsync(c.ValueIds, ct))
                if (set.Remove(id)) excluded.Add(new ScopeExclusion(id, c.Dimension));
        }
        foreach (var id in scope.ExcludeEmployeeIds)
            if (set.Remove(id)) excluded.Add(new ScopeExclusion(id, "EmployeeId"));

        return new ScopeResolution(set.ToList(), excluded, warnings);
    }
}
```

- [ ] **Step 5: Run to verify pass**

```bash
dotnet test backend/tests/HR.Domain.Finance.Tests --filter ScopeEngineTests
```

Expected: PASS (5 tests).

- [ ] **Step 6: Commit**

```bash
git add backend/src/HR.Infrastructure/Engines/Scope backend/tests/HR.Domain.Finance.Tests/ScopeEngineTests.cs
git commit -m "feat(scope): ScopeEngine resolution (OR/AND/exclude-wins/unavailable) + disabled registry"
```

---

## Task 6: Employee scope providers (8 backed + base population) (+ tests)

**Files:**
- Create: `backend/src/HR.Modules/Employees/Scope/EmployeeScopeProviders.cs`
- Test: `backend/tests/HR.Modules.Employees.Tests/EmployeeScopeProvidersTests.cs`

**Interfaces:**
- Consumes: `IScopeDimensionProvider`, `IBasePopulationProvider`, `ScopeDimensionInfo`, `ScopeValueSource` (Task 3); `ApplicationDbContext`, `Employee`, `EmployeeStatus`.
- Produces: `DepartmentScopeProvider`, `BranchScopeProvider`, `JobTitleScopeProvider`, `EmploymentTypeScopeProvider`, `ContractTypeScopeProvider`, `PaymentMethodScopeProvider`, `NationalityScopeProvider`, `StatusScopeProvider`, `ActiveEmployeePopulationProvider`. Discovered via assembly scan in Task 13.

- [ ] **Step 1: Write the failing test**

`EmployeeScopeProvidersTests.cs` (uses the in-memory `ApplicationDbContext` pattern from `HR.Modules.Workflows.Tests/TestHarness.cs` — replicate `FakeCurrentUserService` + `NewContext` locally):

```csharp
using HR.Application.Common.Interfaces;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using HR.Modules.Employees.Scope;
using HR.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Modules.Employees.Tests;

public class EmployeeScopeProvidersTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.View" };
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase($"emp-{Guid.NewGuid()}").Options,
        new FakeUser());

    private static Employee Emp(string num, Guid? dept = null, EmployeeStatus status = EmployeeStatus.Active) => new()
    {
        EmployeeNumber = num, FirstName = num, LastName = "X", Email = $"{num}@t.local",
        DepartmentId = dept, Status = status, BasicSalary = 1000m,
    };

    [Fact]
    public async Task Department_provider_resolves_by_value_ids()
    {
        var sales = Guid.NewGuid();
        await using var db = Ctx();
        var a = Emp("E1", sales); var b = Emp("E2", Guid.NewGuid());
        db.Employees.AddRange(a, b);
        await db.SaveChangesAsync();

        var provider = new DepartmentScopeProvider(db);
        var ids = await provider.ResolveEmployeesAsync(new[] { sales }, default);
        Assert.Equal(new[] { a.Id }, ids);
        Assert.Equal("Department", provider.DimensionKey);
        Assert.True(provider.Info.IsAvailable);
    }

    [Fact]
    public async Task Base_population_excludes_terminated_and_resigned()
    {
        await using var db = Ctx();
        var active = Emp("A1");
        db.Employees.AddRange(active, Emp("T1", status: EmployeeStatus.Terminated), Emp("R1", status: EmployeeStatus.Resigned));
        await db.SaveChangesAsync();

        var ids = await new ActiveEmployeePopulationProvider(db).ResolveAllAsync(default);
        Assert.Equal(new[] { active.Id }, ids);
    }

    [Fact]
    public async Task Status_provider_uses_static_enum_value_source()
    {
        await using var db = Ctx();
        var p = new StatusScopeProvider(db);
        Assert.Equal(ScopeValueSourceKind.StaticEnum, p.Info.ValueSource.Kind);
    }
}
```

- [ ] **Step 2: Run to verify it fails**

```bash
dotnet test backend/tests/HR.Modules.Employees.Tests --filter EmployeeScopeProvidersTests
```

Expected: FAIL — providers don't exist.

- [ ] **Step 3: Implement the providers**

`EmployeeScopeProviders.cs` (one file; each provider is small and column-specific). `Status` maps GUIDs deterministically derived from the enum name so the engine algebra stays GUID-based — values surface to the UI via `StaticEnum`:

```csharp
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Scope;
using HR.Domain.Engines.MasterData;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Employees.Scope;

/// <summary>Base "all active employees" population (mode = All): excludes Terminated/Resigned.</summary>
public sealed class ActiveEmployeePopulationProvider : IBasePopulationProvider
{
    private readonly ApplicationDbContext _db;
    public ActiveEmployeePopulationProvider(ApplicationDbContext db) => _db = db;
    public async Task<ISet<Guid>> ResolveAllAsync(CancellationToken ct) =>
        (await _db.Employees.AsNoTracking()
            .Where(e => e.Status != EmployeeStatus.Terminated && e.Status != EmployeeStatus.Resigned)
            .Select(e => e.Id).ToListAsync(ct)).ToHashSet();
}

internal abstract class EmployeeColumnDimension : IScopeDimensionProvider
{
    protected readonly ApplicationDbContext Db;
    protected EmployeeColumnDimension(ApplicationDbContext db) => Db = db;
    public abstract string DimensionKey { get; }
    public abstract ScopeDimensionInfo Info { get; }
    public abstract Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> valueIds, CancellationToken ct);

    protected static ScopeDimensionInfo MasterDataDim(string key, string en, string ar, string slug) =>
        new(key, en, ar, new ScopeValueSource(ScopeValueSourceKind.MasterData, slug), true, null);

    protected async Task<ISet<Guid>> ByColumn(
        IReadOnlyCollection<Guid> valueIds,
        System.Linq.Expressions.Expression<Func<Employee, bool>> predicate, CancellationToken ct)
    {
        if (valueIds.Count == 0) return new HashSet<Guid>();
        return (await Db.Employees.AsNoTracking().Where(predicate).Select(e => e.Id).ToListAsync(ct)).ToHashSet();
    }
}

public sealed class DepartmentScopeProvider : EmployeeColumnDimension
{
    public DepartmentScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "Department";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Department", "القسم", "departments");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.DepartmentId != null && v.Contains(e.DepartmentId.Value), ct);
}

public sealed class BranchScopeProvider : EmployeeColumnDimension
{
    public BranchScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "Branch";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Branch", "الفرع", "branches");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.BranchId != null && v.Contains(e.BranchId.Value), ct);
}

public sealed class JobTitleScopeProvider : EmployeeColumnDimension
{
    public JobTitleScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "JobTitle";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Job Title", "المسمى الوظيفي", "job-titles");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.JobTitleId != null && v.Contains(e.JobTitleId.Value), ct);
}

public sealed class EmploymentTypeScopeProvider : EmployeeColumnDimension
{
    public EmploymentTypeScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "EmploymentType";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Employment Type", "نوع التوظيف", "employment-types");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.EmploymentTypeId != null && v.Contains(e.EmploymentTypeId.Value), ct);
}

public sealed class ContractTypeScopeProvider : EmployeeColumnDimension
{
    public ContractTypeScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "ContractType";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Contract Type", "نوع العقد", "contract-types");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.ContractTypeId != null && v.Contains(e.ContractTypeId.Value), ct);
}

public sealed class PaymentMethodScopeProvider : EmployeeColumnDimension
{
    public PaymentMethodScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "PaymentMethod";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Payment Method", "طريقة الدفع", "payment-methods");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.PaymentMethodId != null && v.Contains(e.PaymentMethodId.Value), ct);
}

public sealed class NationalityScopeProvider : EmployeeColumnDimension
{
    public NationalityScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "Nationality";
    public override ScopeDimensionInfo Info => MasterDataDim(DimensionKey, "Nationality", "الجنسية", "nationalities");
    public override Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct) =>
        ByColumn(v, e => e.NationalityId != null && v.Contains(e.NationalityId.Value), ct);
}

/// <summary>Status is an enum, not master-data: each EmployeeStatus maps to a deterministic GUID so the
/// engine stays GUID-based. The UI fetches the value list via the StaticEnum value source key "EmployeeStatus".</summary>
public sealed class StatusScopeProvider : EmployeeColumnDimension
{
    public StatusScopeProvider(ApplicationDbContext db) : base(db) { }
    public override string DimensionKey => "Status";
    public override ScopeDimensionInfo Info =>
        new(DimensionKey, "Employment Status", "حالة الموظف",
            new ScopeValueSource(ScopeValueSourceKind.StaticEnum, "EmployeeStatus"), true, null);

    /// <summary>Stable GUID for an EmployeeStatus value (namespace-prefixed by the enum int).</summary>
    public static Guid StatusId(EmployeeStatus s) =>
        new($"00000000-0000-0000-0000-0000000000{(int)s:D2}");

    public override async Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> v, CancellationToken ct)
    {
        if (v.Count == 0) return new HashSet<Guid>();
        var statuses = Enum.GetValues<EmployeeStatus>().Where(s => v.Contains(StatusId(s))).ToList();
        if (statuses.Count == 0) return new HashSet<Guid>();
        return (await Db.Employees.AsNoTracking().Where(e => statuses.Contains(e.Status))
            .Select(e => e.Id).ToListAsync(ct)).ToHashSet();
    }
}
```

> Verify the master-data slugs (`departments`, `branches`, `job-titles`, `employment-types`, `contract-types`, `payment-methods`, `nationalities`) against `MasterDataObjectType.ToSlug(...)` output; fix any that don't match the real lookup slugs.

- [ ] **Step 4: Run to verify pass**

```bash
dotnet test backend/tests/HR.Modules.Employees.Tests --filter EmployeeScopeProvidersTests
```

Expected: PASS (3 tests).

- [ ] **Step 5: Commit**

```bash
git add backend/src/HR.Modules/Employees/Scope backend/tests/HR.Modules.Employees.Tests/EmployeeScopeProvidersTests.cs
git commit -m "feat(scope): Employee dimension providers + base population"
```

---

## Task 7: Wire ScopeEngine + DayBasis into the fact provider; freeze run population (+ tests)

**Files:**
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/PayrollFactProvider.cs`
- Modify: `backend/src/HR.Application/Engines/Finance/IPayrollFactProvider.cs` (signature)
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/PayrollComputation.cs`
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/PayrollRunEngine.cs`
- Test: `backend/tests/HR.Domain.Finance.Tests/DayBasisProrationTests.cs`

**Interfaces:**
- Consumes: `IScopeEngine`, `SelectionScopeJson` (Tasks 3–5); `DayBasis`, `PayrollRunPopulation` (Task 1).
- Produces: `PayrollFactProvider` resolves population via `IScopeEngine`, prorates daily wage by `DayBasis`, and accepts an optional explicit employee-id set; `PayrollRunEngine.CreateAsync` freezes `PayrollRunPopulation` and `CalculateAsync` passes the frozen ids.

- [ ] **Step 1: Write the failing test for DayBasis proration**

`DayBasisProrationTests.cs` (pure helper test — extract proration into a static method so it's unit-testable without a DB):

```csharp
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class DayBasisProrationTests
{
    // monthlyWage = 3000, Feb 2026 has 28 days, 20 working days (example).
    [Theory]
    [InlineData(DayBasis.Fixed30, 3000, 2026, 2, 20, 100.0)]            // 3000/30
    [InlineData(DayBasis.CalendarMonth, 3000, 2026, 2, 20, 107.1429)]  // 3000/28
    [InlineData(DayBasis.WorkingDays, 3000, 2026, 2, 20, 150.0)]       // 3000/20
    public void DailyWage_matches_basis(DayBasis basis, decimal monthlyWage, int year, int month, int workingDays, double expected)
    {
        var daily = PayrollFactProvider.DailyWageFor(basis, monthlyWage, year, month, workingDays);
        Assert.Equal((decimal)expected, decimal.Round(daily, 4));
    }
}
```

- [ ] **Step 2: Run to verify it fails**

```bash
dotnet test backend/tests/HR.Domain.Finance.Tests --filter DayBasisProrationTests
```

Expected: FAIL — `DailyWageFor` does not exist.

- [ ] **Step 3: Add the proration helper + DayBasis to the fact provider**

In `PayrollFactProvider.cs`, add the static helper and accept `DayBasis` + a working-days input. The current code hardcodes `/30m` at line 107 — replace with `DailyWageFor`:

```csharp
    /// <summary>Daily wage for the period under the configured proration basis.</summary>
    public static decimal DailyWageFor(DayBasis basis, decimal monthlyWage, int year, int month, int workingDays)
    {
        var divisor = basis switch
        {
            DayBasis.Fixed30 => 30m,
            DayBasis.CalendarMonth => DateTime.DaysInMonth(year, month),
            DayBasis.WorkingDays => workingDays > 0 ? workingDays : 30m,
            _ => 30m,
        };
        return Math.Round(monthlyWage / divisor, 4);
    }
```

Replace `var dailyWage = Math.Round(monthlyWage / 30m, 4);` with:

```csharp
            var dailyWage = DailyWageFor(version.DayBasis, monthlyWage, period.Start.Year, period.Start.Month,
                att?.Days ?? 0);
```

> `att?.Days` is the worked-days count already aggregated; if you have a dedicated working-days source, prefer it. Worked days is an acceptable proxy for sub-project 1.

- [ ] **Step 4: Replace `ParseFilter` population resolution with the ScopeEngine**

Change the constructor to inject `IScopeEngine`, change the interface signature to accept an optional restriction, and replace the employee query (lines 24–34) with scope resolution. New top of `BuildInputsAsync`:

```csharp
    private readonly ApplicationDbContext _db;
    private readonly IScopeEngine _scope;

    public PayrollFactProvider(ApplicationDbContext db, IScopeEngine scope) { _db = db; _scope = scope; }

    public async Task<IReadOnlyList<EmployeePayrollInput>> BuildInputsAsync(
        PayrollDefinitionVersion version, PayrollPeriod period,
        IReadOnlyCollection<Guid>? restrictToEmployeeIds = null, CancellationToken ct = default)
    {
        HashSet<Guid> empIdSet;
        if (restrictToEmployeeIds is { Count: > 0 })
        {
            empIdSet = restrictToEmployeeIds.ToHashSet();          // run: use the frozen population
        }
        else
        {
            var resolution = await _scope.ResolveAsync(
                HR.Application.Engines.Scope.SelectionScopeJson.Parse(version.SelectionScopeJson), ct);
            empIdSet = resolution.IncludedEmployeeIds.ToHashSet();  // preview: live resolution
        }
        if (empIdSet.Count == 0) return Array.Empty<EmployeePayrollInput>();

        var employees = await _db.Employees.AsNoTracking().Where(e => empIdSet.Contains(e.Id)).ToListAsync(ct);
        // ...rest of the method unchanged (allowances/additions/deductions/attendance/facts)...
```

Delete the now-unused `ParseFilter` method and `EmployeeFilter` record.

Update the interface `IPayrollFactProvider.BuildInputsAsync` to the new signature (add the optional `restrictToEmployeeIds` param).

- [ ] **Step 5: Thread the restriction through PayrollComputation**

In `PayrollComputation.cs`, find `ComputeAsync(version, period, ct)` and its call to `_factProvider.BuildInputsAsync(version, period, ct)`. Add an optional param and forward it:

```csharp
    public async Task<...> ComputeAsync(PayrollDefinitionVersion version, PayrollPeriod period,
        IReadOnlyCollection<Guid>? restrictToEmployeeIds = null, CancellationToken ct = default)
    {
        var inputs = await _factProvider.BuildInputsAsync(version, period, restrictToEmployeeIds, ct);
        // ...unchanged...
    }
```

- [ ] **Step 6: Freeze population in CreateAsync; pass frozen ids in CalculateAsync**

In `PayrollRunEngine.cs`, inject `IScopeEngine` (constructor + field). In `CreateAsync`, after `await _db.SaveChangesAsync(ct);` (run row exists), resolve scope and freeze the population:

```csharp
        // Freeze the resolved population so future org changes never alter this run.
        var resolution = await _scope.ResolveAsync(
            HR.Application.Engines.Scope.SelectionScopeJson.Parse(version.SelectionScopeJson), ct);
        var included = resolution.IncludedEmployeeIds.ToHashSet();
        var snapshotEmployees = await _db.Employees.AsNoTracking()
            .Where(e => included.Contains(e.Id))
            .Select(e => new { e.Id, e.EmployeeNumber, e.FirstName, e.FirstNameAr, e.LastName, e.LastNameAr,
                               e.DepartmentId, e.BranchId, e.JobTitleId, e.PaymentMethodId })
            .ToListAsync(ct);
        foreach (var e in snapshotEmployees)
            _db.PayrollRunPopulations.Add(new PayrollRunPopulation
            {
                PayrollRunId = run.Id, EmployeeId = e.Id,
                EmployeeNumber = e.EmployeeNumber,
                EmployeeName = $"{e.FirstNameAr ?? e.FirstName} {e.LastNameAr ?? e.LastName}".Trim(),
                DepartmentId = e.DepartmentId, BranchId = e.BranchId, JobTitleId = e.JobTitleId,
                PaymentMethodId = e.PaymentMethodId, IsIncluded = true,
            });
        foreach (var ex in resolution.ExcludedByScope)
            _db.PayrollRunPopulations.Add(new PayrollRunPopulation
            {
                PayrollRunId = run.Id, EmployeeId = ex.EmployeeId,
                IsIncluded = false, ExclusionReasonCode = "ExcludedByScope",
            });
        run.EmployeeCount = included.Count;
        await _db.SaveChangesAsync(ct);
```

In `CalculateAsync`, replace `await _computation.ComputeAsync(version, period, ct)` with the frozen-population overload:

```csharp
        var frozen = await _db.PayrollRunPopulations.AsNoTracking()
            .Where(p => p.PayrollRunId == run.Id && p.IsIncluded)
            .Select(p => p.EmployeeId).ToListAsync(ct);
        var computation = await _computation.ComputeAsync(version, period, frozen, ct);
```

(Do the same in `ValidateAsync` so validation runs over the frozen set.)

- [ ] **Step 7: Build + run the finance test suite**

```bash
dotnet build backend/src/HR.Infrastructure/HR.Infrastructure.csproj
dotnet test backend/tests/HR.Domain.Finance.Tests --filter DayBasisProrationTests
```

Expected: build PASS; proration tests PASS.

- [ ] **Step 8: Commit**

```bash
git add backend/src/HR.Application/Engines/Finance/IPayrollFactProvider.cs backend/src/HR.Infrastructure/Engines/Finance backend/tests/HR.Domain.Finance.Tests/DayBasisProrationTests.cs
git commit -m "feat(payroll): resolve population via ScopeEngine, DayBasis proration, frozen run population"
```

---

## Task 8: PayrollTypeService — create/edit, clone, publish, simulate (+ tests)

**Files:**
- Create: `backend/src/HR.Application/Engines/Finance/IPayrollTypeService.cs`
- Create: `backend/src/HR.Infrastructure/Engines/Finance/PayrollTypeService.cs`
- Test: `backend/tests/HR.Domain.Finance.Tests/PayrollTypeServiceTests.cs`

**Interfaces:**
- Consumes: `ApplicationDbContext`, `PayrollDefinition`, `PayrollDefinitionVersion`, `VersionStatus`, `IPayrollPreviewEngine`, `PayrollPeriod`.
- Produces: `IPayrollTypeService` with `CreateTypeAsync`, `UpdateHeaderAsync`, `CreateDraftVersionAsync`, `UpdateDraftVersionAsync`, `CloneVersionAsync`, `PublishVersionAsync`, `SimulateAsync`. Consumed by the controller (Task 9).

- [ ] **Step 1: Write the failing test**

`PayrollTypeServiceTests.cs` (in-memory DbContext, fixed tenant; tests the version state machine — the highest-value behaviour):

```csharp
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTypeServiceTests
{
    private sealed class FakeUser : ICurrentUserService
    {
        public Guid UserId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        public Guid TenantId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string? Email => "t@t.local";
        public IReadOnlyList<string> Permissions { get; } = new[] { "Payroll.Configure" };
        public bool IsAuthenticated => true;
    }

    private static ApplicationDbContext Ctx(string name) => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options, new FakeUser());

    private static PayrollTypeService Svc(ApplicationDbContext db) => new(db, new FakeUser());

    [Fact]
    public async Task Create_type_makes_a_draft_version_v1()
    {
        var name = $"db-{Guid.NewGuid()}";
        await using var db = Ctx(name);
        var svc = Svc(db);
        var id = await svc.CreateTypeAsync(new CreatePayrollTypeArgs("MONTHLY2", "Monthly 2", "شهري", null), default);
        var def = await db.PayrollDefinitions.Include(d => d.Versions).FirstAsync(d => d.Id == id);
        Assert.Single(def.Versions);
        Assert.Equal(VersionStatus.Draft, def.Versions.First().Status);
        Assert.Equal(1, def.Versions.First().VersionNumber);
    }

    [Fact]
    public async Task Publish_supersedes_prior_and_closes_its_EffectiveTo()
    {
        var name = $"db-{Guid.NewGuid()}";
        Guid typeId, v1Id, v2Id;
        await using (var db = Ctx(name))
        {
            var svc = Svc(db);
            typeId = await svc.CreateTypeAsync(new CreatePayrollTypeArgs("M", "M", "م", null), default);
            v1Id = (await db.PayrollDefinitionVersions.FirstAsync(v => v.PayrollDefinitionId == typeId)).Id;
            await svc.PublishVersionAsync(typeId, v1Id, default);
            v2Id = await svc.CloneVersionAsync(typeId, v1Id, default);
            await svc.PublishVersionAsync(typeId, v2Id, default);
        }
        await using (var db = Ctx(name))
        {
            var v1 = await db.PayrollDefinitionVersions.FirstAsync(v => v.Id == v1Id);
            var v2 = await db.PayrollDefinitionVersions.FirstAsync(v => v.Id == v2Id);
            var def = await db.PayrollDefinitions.FirstAsync(d => d.Id == typeId);
            Assert.Equal(VersionStatus.Superseded, v1.Status);
            Assert.NotNull(v1.EffectiveTo);
            Assert.Equal(VersionStatus.Published, v2.Status);
            Assert.Equal(v2Id, def.CurrentVersionId);
        }
    }

    [Fact]
    public async Task Clone_creates_next_version_number_as_draft()
    {
        var name = $"db-{Guid.NewGuid()}";
        await using var db = Ctx(name);
        var svc = Svc(db);
        var typeId = await svc.CreateTypeAsync(new CreatePayrollTypeArgs("M", "M", "م", null), default);
        var v1 = await db.PayrollDefinitionVersions.FirstAsync(v => v.PayrollDefinitionId == typeId);
        await svc.PublishVersionAsync(typeId, v1.Id, default);
        var v2Id = await svc.CloneVersionAsync(typeId, v1.Id, default);
        var v2 = await db.PayrollDefinitionVersions.FirstAsync(v => v.Id == v2Id);
        Assert.Equal(2, v2.VersionNumber);
        Assert.Equal(VersionStatus.Draft, v2.Status);
    }

    [Fact]
    public async Task Cannot_edit_a_published_version()
    {
        var name = $"db-{Guid.NewGuid()}";
        await using var db = Ctx(name);
        var svc = Svc(db);
        var typeId = await svc.CreateTypeAsync(new CreatePayrollTypeArgs("M", "M", "م", null), default);
        var v1 = await db.PayrollDefinitionVersions.FirstAsync(v => v.PayrollDefinitionId == typeId);
        await svc.PublishVersionAsync(typeId, v1.Id, default);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.UpdateDraftVersionAsync(typeId, v1.Id, new UpdatePayrollVersionArgs { CutoffDay = 25 }, default));
    }
}
```

- [ ] **Step 2: Run to verify it fails**

```bash
dotnet test backend/tests/HR.Domain.Finance.Tests --filter PayrollTypeServiceTests
```

Expected: FAIL — service + arg records don't exist.

- [ ] **Step 3: Write the interface + arg records**

`IPayrollTypeService.cs`:

```csharp
using HR.Domain.Engines.Finance;
using HR.Domain.Enums;

namespace HR.Application.Engines.Finance;

public sealed record CreatePayrollTypeArgs(string Code, string Name, string? NameAr, Guid? CategoryId);

public sealed class UpdatePayrollVersionArgs
{
    public int? CutoffDay { get; init; }
    public DayBasis? DayBasis { get; init; }
    public DateTime? ClosingDate { get; init; }
    public DateTime? PaymentDate { get; init; }
    public bool? CarryToNextPeriod { get; init; }
    public Guid? DefaultExportFormatId { get; init; }
    public Guid? PaymentMethodId { get; init; }
    public Guid? ApprovalWorkflowId { get; init; }
    public Guid? RuleSetVersionId { get; init; }
    public string? Currency { get; init; }
    public PayFrequency? Frequency { get; init; }
    public string? SelectionScopeJson { get; init; }
    public string? CalcSettingsJson { get; init; }
    public string? PaymentMethodScopeJson { get; init; }
}

public interface IPayrollTypeService
{
    Task<Guid> CreateTypeAsync(CreatePayrollTypeArgs args, CancellationToken ct);
    Task UpdateHeaderAsync(Guid typeId, string name, string? nameAr, Guid? categoryId, PayrollDefinitionStatus status, CancellationToken ct);
    Task<Guid> CreateDraftVersionAsync(Guid typeId, CancellationToken ct);
    Task UpdateDraftVersionAsync(Guid typeId, Guid versionId, UpdatePayrollVersionArgs args, CancellationToken ct);
    Task<Guid> CloneVersionAsync(Guid typeId, Guid versionId, CancellationToken ct);
    Task PublishVersionAsync(Guid typeId, Guid versionId, CancellationToken ct);
    Task<PayrollPreview> SimulateAsync(Guid typeId, Guid versionId, int year, int month, CancellationToken ct);
}
```

> `PayrollPreview` is the existing preview result type returned by `IPayrollPreviewEngine.PreviewAsync` — confirm its exact name in `PayrollContracts.cs` and use it (do not invent a new type).

- [ ] **Step 4: Implement the service**

`PayrollTypeService.cs`:

```csharp
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>CRUD + version lifecycle for payroll types. Editing is only allowed on Draft versions;
/// publishing supersedes the prior published version and closes its EffectiveTo, preserving history.</summary>
public sealed class PayrollTypeService : IPayrollTypeService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly IPayrollPreviewEngine? _preview;

    // Two ctors: tests use the (db,user) form; DI passes the preview engine too.
    public PayrollTypeService(ApplicationDbContext db, ICurrentUserService user) { _db = db; _user = user; }
    public PayrollTypeService(ApplicationDbContext db, ICurrentUserService user, IPayrollPreviewEngine preview)
        : this(db, user) => _preview = preview;

    public async Task<Guid> CreateTypeAsync(CreatePayrollTypeArgs args, CancellationToken ct)
    {
        var def = new PayrollDefinition
        {
            Code = args.Code, Name = args.Name, NameAr = args.NameAr, CategoryId = args.CategoryId,
            Status = PayrollDefinitionStatus.Draft,
        };
        _db.PayrollDefinitions.Add(def);
        var v = new PayrollDefinitionVersion
        {
            PayrollDefinitionId = def.Id, VersionNumber = 1, Status = VersionStatus.Draft,
            CutoffDay = 27, DayBasis = DayBasis.CalendarMonth, CarryToNextPeriod = true,
        };
        _db.PayrollDefinitionVersions.Add(v);
        await _db.SaveChangesAsync(ct);
        return def.Id;
    }

    public async Task UpdateHeaderAsync(Guid typeId, string name, string? nameAr, Guid? categoryId,
        PayrollDefinitionStatus status, CancellationToken ct)
    {
        var def = await Def(typeId, ct);
        def.Name = name; def.NameAr = nameAr; def.CategoryId = categoryId; def.Status = status;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Guid> CreateDraftVersionAsync(Guid typeId, CancellationToken ct)
    {
        var next = await NextVersionNumber(typeId, ct);
        var v = new PayrollDefinitionVersion
        {
            PayrollDefinitionId = typeId, VersionNumber = next, Status = VersionStatus.Draft,
            CutoffDay = 27, DayBasis = DayBasis.CalendarMonth, CarryToNextPeriod = true,
        };
        _db.PayrollDefinitionVersions.Add(v);
        await _db.SaveChangesAsync(ct);
        return v.Id;
    }

    public async Task UpdateDraftVersionAsync(Guid typeId, Guid versionId, UpdatePayrollVersionArgs a, CancellationToken ct)
    {
        var v = await Version(typeId, versionId, ct);
        if (v.Status != VersionStatus.Draft)
            throw new InvalidOperationException("Only a Draft version can be edited. Clone it first.");
        if (a.CutoffDay is { } cd)
        {
            if (cd < 1 || cd > 31) throw new InvalidOperationException("CutoffDay must be between 1 and 31.");
            v.CutoffDay = cd;
        }
        if (a.DayBasis is { } basis) v.DayBasis = basis;
        if (a.ClosingDate is { } cdt) v.ClosingDate = DateTime.SpecifyKind(cdt, DateTimeKind.Utc);
        if (a.PaymentDate is { } pdt) v.PaymentDate = DateTime.SpecifyKind(pdt, DateTimeKind.Utc);
        if (a.CarryToNextPeriod is { } carry) v.CarryToNextPeriod = carry;
        if (a.DefaultExportFormatId is { } ef) v.DefaultExportFormatId = ef;
        if (a.PaymentMethodId is { } pm) v.PaymentMethodId = pm;
        if (a.ApprovalWorkflowId is { } wf) v.ApprovalWorkflowId = wf;
        if (a.RuleSetVersionId is { } rs) v.RuleSetVersionId = rs;
        if (a.Currency is { } cur) v.Currency = cur;
        if (a.Frequency is { } freq) v.Frequency = freq;
        if (a.SelectionScopeJson is not null) v.SelectionScopeJson = a.SelectionScopeJson;
        if (a.CalcSettingsJson is not null) v.CalcSettingsJson = a.CalcSettingsJson;
        if (a.PaymentMethodScopeJson is not null) v.PaymentMethodScopeJson = a.PaymentMethodScopeJson;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Guid> CloneVersionAsync(Guid typeId, Guid versionId, CancellationToken ct)
    {
        var src = await Version(typeId, versionId, ct);
        var next = await NextVersionNumber(typeId, ct);
        var copy = new PayrollDefinitionVersion
        {
            PayrollDefinitionId = typeId, VersionNumber = next, Status = VersionStatus.Draft,
            Frequency = src.Frequency, Currency = src.Currency, CutoffDay = src.CutoffDay,
            DayBasis = src.DayBasis, ClosingDate = src.ClosingDate, PaymentDate = src.PaymentDate,
            CarryToNextPeriod = src.CarryToNextPeriod, DefaultExportFormatId = src.DefaultExportFormatId,
            PaymentMethodId = src.PaymentMethodId, ApprovalWorkflowId = src.ApprovalWorkflowId,
            RuleSetVersionId = src.RuleSetVersionId, SelectionScopeJson = src.SelectionScopeJson,
            CalcSettingsJson = src.CalcSettingsJson, PaymentMethodScopeJson = src.PaymentMethodScopeJson,
        };
        _db.PayrollDefinitionVersions.Add(copy);
        await _db.SaveChangesAsync(ct);
        return copy.Id;
    }

    public async Task PublishVersionAsync(Guid typeId, Guid versionId, CancellationToken ct)
    {
        var def = await Def(typeId, ct);
        var v = await Version(typeId, versionId, ct);
        if (v.Status == VersionStatus.Superseded)
            throw new InvalidOperationException("A superseded version cannot be published.");

        var now = DateTime.UtcNow;
        if (def.CurrentVersionId is { } currentId && currentId != versionId)
        {
            var prior = await _db.PayrollDefinitionVersions.FirstOrDefaultAsync(x => x.Id == currentId, ct);
            if (prior is not null) { prior.Status = VersionStatus.Superseded; prior.EffectiveTo = now; }
        }
        v.Status = VersionStatus.Published;
        v.PublishedAt = now;
        v.PublishedByUserId = _user.IsAuthenticated ? _user.UserId : null;
        v.EffectiveFrom ??= now;
        def.CurrentVersionId = v.Id;
        if (def.Status == PayrollDefinitionStatus.Draft) def.Status = PayrollDefinitionStatus.Active;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PayrollPreview> SimulateAsync(Guid typeId, Guid versionId, int year, int month, CancellationToken ct)
    {
        _ = await Version(typeId, versionId, ct);
        if (_preview is null) throw new InvalidOperationException("Preview engine not available.");
        return await _preview.PreviewAsync(versionId, PayrollPeriod.Monthly(year, month), ct);
    }

    private async Task<PayrollDefinition> Def(Guid typeId, CancellationToken ct) =>
        await _db.PayrollDefinitions.FirstOrDefaultAsync(d => d.Id == typeId, ct)
        ?? throw new InvalidOperationException($"Payroll type {typeId} not found.");

    private async Task<PayrollDefinitionVersion> Version(Guid typeId, Guid versionId, CancellationToken ct) =>
        await _db.PayrollDefinitionVersions.FirstOrDefaultAsync(v => v.Id == versionId && v.PayrollDefinitionId == typeId, ct)
        ?? throw new InvalidOperationException($"Version {versionId} not found for type {typeId}.");

    private async Task<int> NextVersionNumber(Guid typeId, CancellationToken ct) =>
        (await _db.PayrollDefinitionVersions.Where(v => v.PayrollDefinitionId == typeId)
            .Select(v => (int?)v.VersionNumber).MaxAsync(ct) ?? 0) + 1;
}
```

> Confirm `PayrollPreview` is the real return type of `IPayrollPreviewEngine.PreviewAsync`; adjust the `SimulateAsync` return type to match.

- [ ] **Step 5: Run to verify pass**

```bash
dotnet test backend/tests/HR.Domain.Finance.Tests --filter PayrollTypeServiceTests
```

Expected: PASS (4 tests).

- [ ] **Step 6: Commit**

```bash
git add backend/src/HR.Application/Engines/Finance/IPayrollTypeService.cs backend/src/HR.Infrastructure/Engines/Finance/PayrollTypeService.cs backend/tests/HR.Domain.Finance.Tests/PayrollTypeServiceTests.cs
git commit -m "feat(payroll): PayrollTypeService — create/edit/clone/publish/simulate with version lifecycle"
```

---

## Task 9: Controller endpoints + DTOs + Payroll.Configure permission

**Files:**
- Create: `backend/src/HR.Modules/Payroll/DTOs/PayrollTypeDtos.cs`
- Modify: `backend/src/HR.Modules/Payroll/Controllers/PayrollController.cs`
- Modify: the permission catalog source declaring `Payroll.*` (grep `"Payroll.Lock"` to find it)

**Interfaces:**
- Consumes: `IPayrollTypeService` (Task 8), `IScopeEngine` (Task 5), `SelectionScopeJson` (Task 4), `IPayrollPreviewEngine`.
- Produces: REST endpoints under `api/payroll/types`, `api/payroll/scope/*`.

- [ ] **Step 1: Add the `Payroll.Configure` permission**

In the permission catalog (the constants/list where `Payroll.View/Run/Approve/Lock` are declared — likely `HR.Domain` or `HR.Application` permissions definitions), add `Payroll.Configure` with an Arabic/English label matching the existing entries' shape. Rebuild the permission seeder if there is one.

- [ ] **Step 2: Write the DTOs**

`PayrollTypeDtos.cs`:

```csharp
namespace HR.Modules.Payroll.DTOs;

public sealed class PayrollTypeListItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? NameAr { get; set; }
    public Guid? CategoryId { get; set; }
    public string Status { get; set; } = "";
    public Guid? CurrentVersionId { get; set; }
    public int VersionCount { get; set; }
}

public sealed class PayrollTypeDetailDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? NameAr { get; set; }
    public Guid? CategoryId { get; set; }
    public string Status { get; set; } = "";
    public Guid? CurrentVersionId { get; set; }
    public List<PayrollVersionDto> Versions { get; set; } = new();
}

public sealed class PayrollVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string Status { get; set; } = "";
    public int CutoffDay { get; set; }
    public string DayBasis { get; set; } = "";
    public DateTime? ClosingDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public bool CarryToNextPeriod { get; set; }
    public Guid? DefaultExportFormatId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public Guid? ApprovalWorkflowId { get; set; }
    public Guid? RuleSetVersionId { get; set; }
    public string Currency { get; set; } = "SAR";
    public string Frequency { get; set; } = "";
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? SelectionScopeJson { get; set; }
    public string? CalcSettingsJson { get; set; }
    public string? PaymentMethodScopeJson { get; set; }
}

public sealed class CreateTypeRequest { public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string? NameAr { get; set; } public Guid? CategoryId { get; set; } }
public sealed class UpdateHeaderRequest { public string Name { get; set; } = ""; public string? NameAr { get; set; } public Guid? CategoryId { get; set; } public string Status { get; set; } = "Active"; }
public sealed class UpdateVersionRequest : UpdatePayrollVersionDtoBase { }
public sealed class ScopeDimensionDto { public string Key { get; set; } = ""; public string NameEn { get; set; } = ""; public string NameAr { get; set; } = ""; public string ValueSourceKind { get; set; } = ""; public string? ValueSourceRef { get; set; } public bool IsAvailable { get; set; } public string? UnavailableNote { get; set; } }
public sealed class ResolveScopeRequest { public string ScopeJson { get; set; } = ""; }
public sealed class ResolveScopeResult { public int IncludedCount { get; set; } public int ExcludedCount { get; set; } public List<string> Warnings { get; set; } = new(); }
public sealed class SimulateRequest { public int Year { get; set; } public int Month { get; set; } }

/// <summary>Mirror of UpdatePayrollVersionArgs over the wire.</summary>
public abstract class UpdatePayrollVersionDtoBase
{
    public int? CutoffDay { get; set; }
    public string? DayBasis { get; set; }
    public DateTime? ClosingDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public bool? CarryToNextPeriod { get; set; }
    public Guid? DefaultExportFormatId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public Guid? ApprovalWorkflowId { get; set; }
    public Guid? RuleSetVersionId { get; set; }
    public string? Currency { get; set; }
    public string? Frequency { get; set; }
    public string? SelectionScopeJson { get; set; }
    public string? CalcSettingsJson { get; set; }
    public string? PaymentMethodScopeJson { get; set; }
}
```

- [ ] **Step 3: Add the endpoints to the controller**

Inject `IPayrollTypeService _types`, `IScopeEngine _scope` into `PayrollController` (extend the constructor + fields). Add:

```csharp
    [HttpGet("types")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<List<PayrollTypeListItem>>>> Types(CancellationToken ct)
    {
        var list = await _db.PayrollDefinitions.AsNoTracking().OrderBy(d => d.Name)
            .Select(d => new PayrollTypeListItem
            {
                Id = d.Id, Code = d.Code, Name = d.Name, NameAr = d.NameAr, CategoryId = d.CategoryId,
                Status = d.Status.ToString(), CurrentVersionId = d.CurrentVersionId,
                VersionCount = d.Versions.Count,
            }).ToListAsync(ct);
        return OkResponse(list);
    }

    [HttpGet("types/{id:guid}")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<PayrollTypeDetailDto>>> Type(Guid id, CancellationToken ct)
    {
        var d = await _db.PayrollDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("PayrollType", id);
        var versions = await _db.PayrollDefinitionVersions.AsNoTracking()
            .Where(v => v.PayrollDefinitionId == id).OrderBy(v => v.VersionNumber)
            .Select(v => new PayrollVersionDto
            {
                Id = v.Id, VersionNumber = v.VersionNumber, Status = v.Status.ToString(),
                CutoffDay = v.CutoffDay, DayBasis = v.DayBasis.ToString(), ClosingDate = v.ClosingDate,
                PaymentDate = v.PaymentDate, CarryToNextPeriod = v.CarryToNextPeriod,
                DefaultExportFormatId = v.DefaultExportFormatId, PaymentMethodId = v.PaymentMethodId,
                ApprovalWorkflowId = v.ApprovalWorkflowId, RuleSetVersionId = v.RuleSetVersionId,
                Currency = v.Currency, Frequency = v.Frequency.ToString(),
                EffectiveFrom = v.EffectiveFrom, EffectiveTo = v.EffectiveTo,
                SelectionScopeJson = v.SelectionScopeJson, CalcSettingsJson = v.CalcSettingsJson,
                PaymentMethodScopeJson = v.PaymentMethodScopeJson,
            }).ToListAsync(ct);
        return OkResponse(new PayrollTypeDetailDto
        {
            Id = d.Id, Code = d.Code, Name = d.Name, NameAr = d.NameAr, CategoryId = d.CategoryId,
            Status = d.Status.ToString(), CurrentVersionId = d.CurrentVersionId, Versions = versions,
        });
    }

    [HttpPost("types")]
    [RequirePermission("Payroll.Configure")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateType([FromBody] CreateTypeRequest req, CancellationToken ct)
        => CreatedResponse(await _types.CreateTypeAsync(new CreatePayrollTypeArgs(req.Code, req.Name, req.NameAr, req.CategoryId), ct));

    [HttpPut("types/{id:guid}")]
    [RequirePermission("Payroll.Configure")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateHeader(Guid id, [FromBody] UpdateHeaderRequest req, CancellationToken ct)
    {
        var status = Enum.TryParse<PayrollDefinitionStatus>(req.Status, out var s) ? s : PayrollDefinitionStatus.Active;
        await _types.UpdateHeaderAsync(id, req.Name, req.NameAr, req.CategoryId, status, ct);
        return OkResponse(true);
    }

    [HttpPost("types/{id:guid}/versions")]
    [RequirePermission("Payroll.Configure")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateVersion(Guid id, CancellationToken ct)
        => CreatedResponse(await _types.CreateDraftVersionAsync(id, ct));

    [HttpPut("types/{id:guid}/versions/{vid:guid}")]
    [RequirePermission("Payroll.Configure")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateVersion(Guid id, Guid vid, [FromBody] UpdateVersionRequest req, CancellationToken ct)
    {
        await _types.UpdateDraftVersionAsync(id, vid, new UpdatePayrollVersionArgs
        {
            CutoffDay = req.CutoffDay,
            DayBasis = Enum.TryParse<DayBasis>(req.DayBasis, out var b) ? b : null,
            ClosingDate = req.ClosingDate, PaymentDate = req.PaymentDate, CarryToNextPeriod = req.CarryToNextPeriod,
            DefaultExportFormatId = req.DefaultExportFormatId, PaymentMethodId = req.PaymentMethodId,
            ApprovalWorkflowId = req.ApprovalWorkflowId, RuleSetVersionId = req.RuleSetVersionId,
            Currency = req.Currency,
            Frequency = Enum.TryParse<PayFrequency>(req.Frequency, out var f) ? f : null,
            SelectionScopeJson = req.SelectionScopeJson, CalcSettingsJson = req.CalcSettingsJson,
            PaymentMethodScopeJson = req.PaymentMethodScopeJson,
        }, ct);
        return OkResponse(true);
    }

    [HttpPost("types/{id:guid}/versions/{vid:guid}/clone")]
    [RequirePermission("Payroll.Configure")]
    public async Task<ActionResult<ApiResponse<Guid>>> CloneVersion(Guid id, Guid vid, CancellationToken ct)
        => CreatedResponse(await _types.CloneVersionAsync(id, vid, ct));

    [HttpPost("types/{id:guid}/versions/{vid:guid}/publish")]
    [RequirePermission("Payroll.Configure")]
    public async Task<ActionResult<ApiResponse<bool>>> PublishVersion(Guid id, Guid vid, CancellationToken ct)
    {
        await _types.PublishVersionAsync(id, vid, ct);
        return OkResponse(true);
    }

    [HttpPost("types/{id:guid}/versions/{vid:guid}/simulate")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<PayrollPreviewDto>>> Simulate(Guid id, Guid vid, [FromBody] SimulateRequest req, CancellationToken ct)
    {
        var preview = await _types.SimulateAsync(id, vid, req.Year, req.Month, ct);
        return OkResponse(new PayrollPreviewDto
        {
            EmployeeCount = preview.EmployeeCount, GrossTotal = preview.GrossTotal,
            DeductionTotal = preview.DeductionTotal, NetTotal = preview.NetTotal, Currency = preview.Currency,
            IsValid = preview.Validation.IsValid,
            Findings = preview.Validation.Findings.Select(ToFindingDto).ToList(),
            Lines = preview.Lines.Select(l => new PayrollPreviewLineDto
            {
                EmployeeId = l.EmployeeId, EmployeeNumber = l.EmployeeNumber, EmployeeName = l.EmployeeName,
                Gross = l.Gross, Deductions = l.Deductions, Net = l.Net, HasErrors = l.HasErrors,
            }).ToList(),
        });
    }

    [HttpGet("scope/dimensions")]
    [RequirePermission("Payroll.View")]
    public ActionResult<ApiResponse<List<ScopeDimensionDto>>> ScopeDimensions()
        => OkResponse(_scope.Dimensions().Select(d => new ScopeDimensionDto
        {
            Key = d.Key, NameEn = d.NameEn, NameAr = d.NameAr,
            ValueSourceKind = d.ValueSource.Kind.ToString(), ValueSourceRef = d.ValueSource.Reference,
            IsAvailable = d.IsAvailable, UnavailableNote = d.UnavailableNote,
        }).ToList());

    [HttpPost("scope/resolve")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<ResolveScopeResult>>> ResolveScope([FromBody] ResolveScopeRequest req, CancellationToken ct)
    {
        var resolution = await _scope.ResolveAsync(SelectionScopeJson.Parse(req.ScopeJson), ct);
        return OkResponse(new ResolveScopeResult
        {
            IncludedCount = resolution.IncludedEmployeeIds.Count,
            ExcludedCount = resolution.ExcludedByScope.Count,
            Warnings = resolution.Warnings.ToList(),
        });
    }
```

Add the needed `using` lines: `HR.Application.Engines.Finance`, `HR.Application.Engines.Scope`, `HR.Domain.Enums`.

- [ ] **Step 4: Build**

```bash
dotnet build backend/src/HR.Modules/HR.Modules.csproj
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add backend/src/HR.Modules backend/src/HR.Domain backend/src/HR.Application
git commit -m "feat(payroll): type/version/scope HTTP endpoints + Payroll.Configure permission"
```

---

## Task 10: Seeder — stamp the standard MONTHLY type with defaults

**Files:**
- Modify: `backend/src/HR.Infrastructure/Engines/Finance/StandardPayrollSeeder.cs`

**Interfaces:**
- Consumes: master-data `PayrollTypeCategory`/`PayrollExportFormat` rows (Task 2); the new version columns (Task 1).

- [ ] **Step 1: Stamp defaults when seeding the MONTHLY definition version**

In `StandardPayrollSeeder.cs`, where the `MONTHLY` `PayrollDefinitionVersion` is created/topped-up, set the new fields and link the category + export format by looking up their master-data ids:

```csharp
        var categoryId = await _db.MasterDataItems
            .Where(m => m.ObjectType == MasterDataObjectType.PayrollTypeCategory && m.Code == "REGULAR")
            .Select(m => (Guid?)m.Id).FirstOrDefaultAsync(ct);
        var pdfFormatId = await _db.MasterDataItems
            .Where(m => m.ObjectType == MasterDataObjectType.PayrollExportFormat && m.Code == "PDF")
            .Select(m => (Guid?)m.Id).FirstOrDefaultAsync(ct);

        // On the definition:
        definition.CategoryId ??= categoryId;
        // On the version (when first created):
        version.CutoffDay = 27;
        version.DayBasis = DayBasis.CalendarMonth;
        version.CarryToNextPeriod = true;
        version.DefaultExportFormatId ??= pdfFormatId;
        version.SelectionScopeJson ??= "{\"mode\":\"All\",\"include\":[],\"exclude\":[],\"includeEmployeeIds\":[],\"excludeEmployeeIds\":[]}";
```

> Ensure `MasterDataObjectType` and `DayBasis` are imported. If the seeder seeds master-data defaults itself, ensure the catalog rows from Task 2 are seeded before this lookup (or call the master-data default seeding first).

- [ ] **Step 2: Build**

```bash
dotnet build backend/src/HR.Infrastructure/HR.Infrastructure.csproj
```

Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add backend/src/HR.Infrastructure/Engines/Finance/StandardPayrollSeeder.cs
git commit -m "feat(payroll): seed MONTHLY type with category, export format, cutoff, day-basis, scope=All"
```

---

## Task 11: DI wiring + full backend build & test

**Files:**
- Modify: `backend/src/HR.Infrastructure/DependencyInjection.cs`
- Modify: `backend/src/HR.Modules/Employees/DependencyInjection/DependencyInjection.cs`

**Interfaces:**
- Consumes: everything above.

- [ ] **Step 1: Register the scope engine, providers, and type service**

In `HR.Infrastructure/DependencyInjection.cs`, in the Financial Engine block (after line ~67), add:

```csharp
        // Scope engine (pluggable dimension providers). Payroll depends only on IScopeEngine.
        services.AddScoped<HR.Application.Engines.Scope.IScopeEngine, HR.Infrastructure.Engines.Scope.ScopeEngine>();
        services.AddScoped<HR.Application.Engines.Finance.IPayrollTypeService, HR.Infrastructure.Engines.Finance.PayrollTypeService>();
```

> `PayrollTypeService` has two constructors; the DI container picks the greediest it can satisfy (the 3-arg one with `IPayrollPreviewEngine`), which is what we want. `IScopeDimensionProvider`/`IBasePopulationProvider` come from the Employees module assembly scan (next step). If no Employees scan runs in a given host, `ScopeEngine`'s `IBasePopulationProvider` dependency would be unsatisfied — the Employees registration below guarantees it.

- [ ] **Step 2: Scan the Employees assembly for scope providers**

In `HR.Modules/Employees/DependencyInjection/DependencyInjection.cs`, add inside the module registration method:

```csharp
        services.AddScopeProvidersFromAssembly(typeof(HR.Modules.Employees.Scope.DepartmentScopeProvider).Assembly);
```

Add `using HR.Application.Engines.Scope;`.

- [ ] **Step 3: Full build**

```bash
dotnet build backend/HR.sln
```

Expected: PASS (0 errors).

- [ ] **Step 4: Full test run**

```bash
dotnet test backend/HR.sln
```

Expected: all existing 88 tests + the new ScopeEngine/SelectionScopeJson/PayrollTypeService/DayBasis/EmployeeScopeProviders tests PASS.

- [ ] **Step 5: Apply the migration to the Azure DB**

```bash
dotnet ef database update -p backend/src/HR.Infrastructure -s backend/src/HR.Api --connection "$env:ConnectionStrings__DefaultConnection"
```

Expected: `PayrollTypesAndScope` applies cleanly; backfill SQL runs.

- [ ] **Step 6: Commit**

```bash
git add backend/src/HR.Infrastructure/DependencyInjection.cs backend/src/HR.Modules/Employees/DependencyInjection/DependencyInjection.cs
git commit -m "feat(payroll): register ScopeEngine, PayrollTypeService, Employee scope providers"
```

---

## Task 12: Frontend — API client + Payroll Types list page

**Files:**
- Create: `src/lib/api/payroll-types.ts`
- Create: `src/app/(dashboard)/settings/payroll/types/page.tsx`
- Modify: `src/app/(dashboard)/settings/payroll/page.tsx` (add a link card to "Payroll Types")

**Interfaces:**
- Consumes: `api/payroll/types*` and `api/payroll/scope/*` endpoints (Task 9). Follow the existing `src/lib/api/payroll.ts` client structure (fetch wrapper, `ApiResponse` unwrap, auth header).

- [ ] **Step 1: Write the API client**

`src/lib/api/payroll-types.ts` — replicate the request helper from `src/lib/api/payroll.ts` (import its shared `apiFetch`/base if exported; otherwise mirror it). Types + functions:

```ts
export type ScopeDimension = {
  key: string; nameEn: string; nameAr: string;
  valueSourceKind: "MasterData" | "StaticEnum" | "Custom";
  valueSourceRef: string | null; isAvailable: boolean; unavailableNote: string | null;
};
export type PayrollTypeListItem = {
  id: string; code: string; name: string; nameAr: string | null;
  categoryId: string | null; status: string; currentVersionId: string | null; versionCount: number;
};
export type PayrollVersion = {
  id: string; versionNumber: number; status: string; cutoffDay: number; dayBasis: string;
  closingDate: string | null; paymentDate: string | null; carryToNextPeriod: boolean;
  defaultExportFormatId: string | null; paymentMethodId: string | null; approvalWorkflowId: string | null;
  ruleSetVersionId: string | null; currency: string; frequency: string;
  effectiveFrom: string | null; effectiveTo: string | null;
  selectionScopeJson: string | null; calcSettingsJson: string | null; paymentMethodScopeJson: string | null;
};
export type PayrollTypeDetail = PayrollTypeListItem & { versions: PayrollVersion[] };

export const payrollTypesApi = {
  list: () => apiGet<PayrollTypeListItem[]>("/api/payroll/types"),
  get: (id: string) => apiGet<PayrollTypeDetail>(`/api/payroll/types/${id}`),
  create: (body: { code: string; name: string; nameAr?: string; categoryId?: string }) =>
    apiPost<string>("/api/payroll/types", body),
  updateHeader: (id: string, body: { name: string; nameAr?: string; categoryId?: string; status: string }) =>
    apiPut<boolean>(`/api/payroll/types/${id}`, body),
  createVersion: (id: string) => apiPost<string>(`/api/payroll/types/${id}/versions`, {}),
  updateVersion: (id: string, vid: string, body: Partial<PayrollVersion>) =>
    apiPut<boolean>(`/api/payroll/types/${id}/versions/${vid}`, body),
  cloneVersion: (id: string, vid: string) => apiPost<string>(`/api/payroll/types/${id}/versions/${vid}/clone`, {}),
  publishVersion: (id: string, vid: string) => apiPost<boolean>(`/api/payroll/types/${id}/versions/${vid}/publish`, {}),
  simulate: (id: string, vid: string, year: number, month: number) =>
    apiPost(`/api/payroll/types/${id}/versions/${vid}/simulate`, { year, month }),
  scopeDimensions: () => apiGet<ScopeDimension[]>("/api/payroll/scope/dimensions"),
  resolveScope: (scopeJson: string) =>
    apiPost<{ includedCount: number; excludedCount: number; warnings: string[] }>("/api/payroll/scope/resolve", { scopeJson }),
};
```

> Use the actual exported helpers from `src/lib/api/payroll.ts` (e.g. it may export a single `request()` — adapt names). Do not invent `apiGet/apiPost/apiPut` if the project uses a different convention.

- [ ] **Step 2: Write the list page**

`src/app/(dashboard)/settings/payroll/types/page.tsx` — an `AccessGuard`-wrapped (`Payroll.View`) RTL page that lists types (code, AR name, status badge, version count) with a "New type" dialog gated on `Payroll.Configure` (use `usePermissions` as the existing pages do). Model the layout on `src/app/(dashboard)/settings/payroll/allowances/page.tsx` (read it first for the table/dialog idiom). The "New type" form posts `{ code, name, nameAr, categoryId }` (category from the `payroll-type-categories` master-data lookup) and routes to `/settings/payroll/types/{id}` on success.

- [ ] **Step 3: Add a link card on the payroll-settings hub**

In `src/app/(dashboard)/settings/payroll/page.tsx`, add a card linking to `/settings/payroll/types` titled "أنواع المسير / Payroll Types".

- [ ] **Step 4: Verify the frontend builds**

```bash
npm run build
```

Expected: build succeeds (or `npm run lint` / typecheck if that's the project's gate — match the repo's CI command).

- [ ] **Step 5: Commit**

```bash
git add src/lib/api/payroll-types.ts "src/app/(dashboard)/settings/payroll/types/page.tsx" "src/app/(dashboard)/settings/payroll/page.tsx"
git commit -m "feat(payroll-ui): payroll-types API client + list page + settings hub link"
```

---

## Task 13: Frontend — Type detail, version editor tabs, scope builder

**Files:**
- Create: `src/app/(dashboard)/settings/payroll/types/[id]/page.tsx`
- Create: `src/components/payroll/scope-builder.tsx`

**Interfaces:**
- Consumes: `payrollTypesApi` (Task 12).

- [ ] **Step 1: Write the scope builder component**

`src/components/payroll/scope-builder.tsx` — props `{ value: string | null; onChange: (json: string) => void }`. It:
- fetches `payrollTypesApi.scopeDimensions()`;
- renders a **mode** toggle (All / Criteria);
- under Criteria, renders **Include** and **Exclude** sections; each lets the user pick a dimension (available ones enabled; disabled ones shown greyed with `unavailableNote` tooltip) and select values. For `valueSourceKind === "MasterData"`, fetch values from the existing lookups endpoint using `valueSourceRef` (the slug) — reuse the project's lookup hook/client. For `StaticEnum` (`EmployeeStatus`), render the known status options;
- serializes selections to the `SelectionScope` JSON shape (`{mode,include,exclude,includeEmployeeIds,excludeEmployeeIds}`) and calls `onChange`;
- shows a **live count** by calling `payrollTypesApi.resolveScope(json)` (debounced) → "X included · Y excluded" plus any warnings.

Build the GUID mapping for `EmployeeStatus` to match the backend `StatusScopeProvider.StatusId` formula (`00000000-0000-0000-0000-0000000000NN` where NN is the 2-digit enum int) so selected statuses resolve. Document this contract in a comment.

- [ ] **Step 2: Write the type detail + version editor page**

`src/app/(dashboard)/settings/payroll/types/[id]/page.tsx` — `AccessGuard` `Payroll.View`; mutations gated `Payroll.Configure`. Layout:
- **Header**: code, editable name/nameAr/category/status (PUT header).
- **Version timeline**: list versions (number, status badge, EffectiveFrom/To). Actions per version: **Clone** (any), **Publish** (Draft only), **Simulate** (opens a year/month dialog → shows preview totals). Selecting a version opens the editor.
- **Version editor** (Draft only; published versions render read-only with a "Clone to edit" button) — tabs:
  - *General*: frequency, currency, payment method (lookup), default export format (lookup `payroll-export-formats`), approval workflow (lookup/existing workflow picker), rule-set version (existing picker or id field).
  - *Selection Scope*: `<ScopeBuilder value={version.selectionScopeJson} onChange={...} />`.
  - *Calculation*: day basis (CalendarMonth/Fixed30/WorkingDays) + the toggle set serialized to `calcSettingsJson` + excluded-allowance multiselect.
  - *Cutoff*: cutoff day (1–31), closing date, payment date, carry-over toggle, and the derived message "تُرحّل الحركات بعد يوم {cutoffDay} إلى الفترة التالية / Transactions after day {cutoffDay} carry to the next period."
- "Save" calls `updateVersion(id, vid, body)` with the changed fields.

Model dialog/tab/lookup idioms on the existing `settings/payroll/*` pages.

- [ ] **Step 3: Verify build**

```bash
npm run build
```

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add "src/app/(dashboard)/settings/payroll/types/[id]/page.tsx" src/components/payroll/scope-builder.tsx
git commit -m "feat(payroll-ui): type detail, version editor tabs, scope builder with live count"
```

---

## Task 14: End-to-end verification (acceptance for sub-project 1)

**Files:** none (manual + smoke verification).

- [ ] **Step 1: Backend smoke**

Run the API locally (or against the deployed App Service). With a `Payroll.Configure` token:
- `POST /api/payroll/types` `{code:"OPS", name:"Ops Monthly", nameAr:"شهري العمليات", categoryId:<REGULAR id>}` → 201, returns id.
- `PUT /api/payroll/types/{id}/versions/{vid}` with a Department-include scope + `cutoffDay:27` + `dayBasis:"CalendarMonth"`.
- `POST /api/payroll/scope/resolve` with that scope → `includedCount > 0` matching the department.
- `POST /api/payroll/types/{id}/versions/{vid}/publish` → 200; `GET /types/{id}` shows version Published with `CurrentVersionId` set.
- `POST /api/payroll/runs` `{definitionId:{id}, year, month}` → run created; `GET /api/payroll/runs/{runId}` reflects `employeeCount` equal to the resolved population; the `engine_payroll_run_population` table has frozen rows.

- [ ] **Step 2: Confirm immutability**

After creating the run, change an employee's department in the DB, re-`GET` the run → `employeeCount` and the frozen population are unchanged (proves the snapshot).

- [ ] **Step 3: Frontend smoke**

`/settings/payroll/types` lists the new type; open it; the scope builder shows available dimensions + the 7 disabled ones (greyed, with notes); the live count updates as criteria change; Clone/Publish/Simulate work.

- [ ] **Step 4: Full regression**

```bash
dotnet test backend/HR.sln
```

Expected: all green.

- [ ] **Step 5: Final commit / branch wrap**

```bash
git add -A
git commit -m "test(payroll): sub-project 1 end-to-end verification notes"
```

---

## Self-Review

**Spec coverage** (against `2026-06-30-payroll-types-scope-cutoff-design.md`):
- Configurable catalogs (Category/Export-Format) → Task 2. ✓
- Typed columns + JSON on version; CategoryId; `PayrollRunPopulation` → Task 1. ✓
- Scope abstractions / provider pattern / disabled registry → Tasks 3, 5. ✓
- SelectionScope JSON → Task 4. ✓
- Employee providers (8 backed + base) → Task 6. ✓
- Resolution semantics (OR/AND/exclude-wins/unavailable/mode=All) → Task 5 tests. ✓
- Fact-provider integration + DayBasis proration + frozen population at create/calculate → Task 7. ✓
- Versioning (clone/publish/simulate, EffectiveTo closure) → Task 8. ✓
- API surface + `Payroll.Configure` → Task 9. ✓
- Seeder defaults + backfill → Tasks 1 (backfill), 10 (seed). ✓
- Frontend (list, detail/editor tabs, scope builder, live count) → Tasks 12, 13. ✓
- Tests (scope resolution, snapshot freeze, clone/publish, day-basis, backfill) → Tasks 4–8; backfill verified via migration script (Task 1) + e2e (Task 14). ✓

**Placeholder scan:** no "TBD"/"implement later". The few "confirm/adjust against the real file" notes are explicit verification steps (record/helper shapes that must match existing code), not deferred work — each names the exact file to check.

**Type consistency:** `SelectionScope`, `ScopeCriterion`, `ScopeResolution`, `ScopeDimensionInfo`, `ScopeValueSource(Kind)`, `IScopeEngine.Dimensions()/ResolveAsync`, `IScopeDimensionProvider.ResolveEmployeesAsync`, `IBasePopulationProvider.ResolveAllAsync`, `SelectionScopeJson.Parse/Serialize`, `PayrollFactProvider.DailyWageFor/BuildInputsAsync(..., restrictToEmployeeIds, ...)`, `IPayrollTypeService` method names, and `UpdatePayrollVersionArgs` fields are used identically across tasks. `StatusScopeProvider.StatusId` GUID formula is referenced by the frontend (Task 13) with the same shape.

**Known external dependencies to verify during execution** (flagged inline, not gaps): exact `MasterDataDefault` constructor; the finance EF-config registration idiom; `PayrollPreview` type name; `src/lib/api/payroll.ts` request-helper names; the permission-catalog source file.

# Sub-project 1 — Payroll Types + Selection Scope + Cutoff

**Date:** 2026-06-30
**Parent:** `2026-06-30-payroll-engine-redesign-master.md`
**Status:** Design approved — ready for implementation plan

## Summary

Turn the existing `PayrollDefinition`/`PayrollDefinitionVersion` into a fully
configurable, customer-extensible **Payroll Type** with rich employee selection
(pluggable Scope Engine), calculation settings, cutoff configuration, and config
versioning (clone/publish/simulate). Every run snapshots its resolved employee
population. No new top-level "type" entity — the definition *is* the type.

Enforcement that depends on dated transactions (cutoff carry-over, component-gating
calc toggles) is **configured and persisted here** but **enforced in sub-projects 2/3**
when additions/deductions/loans exist. The only calc semantic wired now is `DayBasis`
proration (no transaction dependency).

## Design decisions (resolved during brainstorming)

- **D1.** Payroll Type = enriched `PayrollDefinition` (+ version). No parallel entity;
  respects no-duplicate-fields schema governance.
- **D2.** New config goes on the **immutable `PayrollDefinitionVersion`** snapshot
  (versioned, reproducible), not the mutable header — except header identity fields.
- **D3.** Hot/queryable fields = **typed columns**; flexible/advanced = **JSON**.
- **D4.** `Category` and `ExportFormat` become **configurable master-data catalogs**
  (new `MasterDataObjectType`s), not C# enums. Payment methods already master-data.
- **D5.** Selection scope is a **dimension registry + pluggable providers** (Strategy).
  Payroll calls `IScopeEngine` only; each owning module supplies its dimension's
  resolver. Disabled dimensions appear in the registry with a "backing provider not yet
  available" note. Payroll is **never** coupled to Employee columns.
- **D6.** Backed dimensions activated now: Department, Branch, Job Title, Employment
  Type, Contract Type, Payment Method, Status, Nationality (+ base "all active"
  population). Unbacked (Tag, CostCenter, Grade, Shift, Project, BusinessUnit, Company)
  registered but disabled.
- **D7.** Every run snapshots its resolved population (`PayrollRunPopulation`) at
  creation; org changes never alter historical runs.

## Data model

### New configurable catalogs (master-data `ObjectType`s)

Managed through the existing `api/platform/master-data` UI; seeded via
`MasterDataDefaults`.

- **`PayrollTypeCategory`** — rows: `Regular`, `Mudad`, `Cash`, `Bonus`,
  `EndOfService`, `OffCycle` (extensible). `MetadataJson`: `{ defaultExportFormatCode,
  defaultPaymentMethodScope[] }`.
- **`PayrollExportFormat`** — rows: `PDF`, `Excel`, `CSV`, `TXT`, `BankTransfer`,
  `Mudad`, `Cash`. `MetadataJson.handlerKey` maps the row to a code-registered exporter
  handler. Customers may rename/add/reorder/disable formats and choose a built-in
  handler; a genuinely new file format still requires a code handler (documented
  boundary).

### `PayrollDefinition` (header, mutable) — additions

- `CategoryId : Guid?` → master-data `PayrollTypeCategory` FK.
- Existing reused: `Code`, `NameAr`, `NameEn`, `Description`, `Status`
  (Active/Inactive), `Scope`.

### `PayrollDefinitionVersion` (immutable snapshot)

New **typed columns**:

| Column | Type | Notes |
|---|---|---|
| `CutoffDay` | `int` | 1–31, validated |
| `DayBasis` | `DayBasis` enum | `CalendarMonth` / `Fixed30` / `WorkingDays` |
| `ClosingDate` | `DateTime?` | period close |
| `PaymentDate` | `DateTime?` | scheduled pay date |
| `CarryToNextPeriod` | `bool` | carry post-cutoff txns (enforced sub-project 2/3) |
| `DefaultExportFormatId` | `Guid?` | master-data `PayrollExportFormat` FK |
| `EffectiveFrom` | `DateTime?` | versioning metadata |
| `EffectiveTo` | `DateTime?` | closed when a newer version publishes |
| `IsSimulation` | `bool` | draft used for dry-run only |

New **JSON columns**:

- `SelectionScopeJson` — see Scope Engine below.
- `CalcSettingsJson` — `{ excludedAllowanceTypeIds[], additionsInGross,
  includeAllowances, includeAdditions, includeDeductions, includeAttendanceDeductions,
  includeLoans, includeGosi, includeUnpaidLeave, includeOvertime }`. (`dayBasis` is the
  typed column above, not in JSON.)
- `PaymentMethodScopeJson` — allowed master-data PaymentMethod ids for this type.

Reused (not duplicated): `Frequency`, `Currency`, `ApprovalWorkflowId`,
`PaymentMethodId`, `RuleSetVersionId` (pins calc behaviour), `VersionNumber`, `Status`,
`PublishedAt`.

> Legacy `EmployeeFilterJson` is migrated into `SelectionScopeJson` and then no longer
> read (kept nullable for safety; can be dropped in a later cleanup migration).

### `PayrollRunPopulation` (new child table `engine_payroll_run_population`)

One frozen row per resolved employee at run creation:

- `PayrollRunId`, `EmployeeId`
- Snapshotted attributes: `EmployeeNumber`, `EmployeeName`, `DepartmentId`,
  `BranchId`, `JobTitleId`, `PaymentMethodId` (extensible set used by later
  exclusion/exports).
- `IsIncluded : bool`
- `ExclusionReasonCode : string?` — sub-project 1 sets only scope-based reasons
  (e.g. `ExcludedByScope`); validity reasons (no contract, no salary, on unpaid
  leave, …) are enriched in sub-project 3.

### Enums

- `DayBasis { CalendarMonth = 1, Fixed30 = 2, WorkingDays = 3 }`.

## Scope Engine (pluggable providers)

Interfaces in `HR.Application/Engines/Scope/` (referenceable by any module, no circular
deps):

```csharp
public sealed record ScopeDimensionInfo(
    string Key, string NameEn, string NameAr,
    ScopeValueSource ValueSource,   // MasterData(objectTypeSlug) | StaticEnum | Custom
    bool IsAvailable, string? UnavailableNote);

public interface IScopeDimensionProvider {
    string DimensionKey { get; }
    ScopeDimensionInfo Info { get; }
    Task<ISet<Guid>> ResolveEmployeesAsync(
        IReadOnlyCollection<Guid> valueIds, CancellationToken ct);
}

public interface IBasePopulationProvider {           // owns "all active employees"
    Task<ISet<Guid>> ResolveAllAsync(CancellationToken ct);
}

public interface IScopeEngine {
    IReadOnlyList<ScopeDimensionInfo> Dimensions();
    Task<ScopeResolution> ResolveAsync(SelectionScope scope, CancellationToken ct);
}

public sealed record ScopeResolution(
    IReadOnlyCollection<Guid> IncludedEmployeeIds,
    IReadOnlyCollection<ScopeExclusion> ExcludedByScope,   // id + dimension key
    IReadOnlyCollection<string> Warnings);                 // e.g. unavailable dimension
```

`SelectionScope` (deserialized from `SelectionScopeJson`):

```jsonc
{
  "mode": "All" | "Criteria",
  "include": [ { "dimension": "Department", "valueIds": ["..."] } ],
  "exclude": [ { "dimension": "Status", "valueIds": ["..."], "employeeIds": ["..."] } ],
  "includeEmployeeIds": ["..."],
  "excludeEmployeeIds": ["..."]
}
```

### Resolution semantics (`ScopeEngine.ResolveAsync`)

1. Start set = `mode == All` ? `IBasePopulationProvider.ResolveAllAsync()` : empty.
2. **Includes:** per criterion, owning provider resolves valueIds → employee set.
   Within a dimension = **OR** (provider unions valueIds); across dimensions = **AND**
   (engine intersects successive dimension sets). `includeEmployeeIds` unioned in.
3. **Excludes:** each exclude criterion's resolved set subtracted;
   `excludeEmployeeIds` subtracted. **Exclude always wins.**
4. A referenced dimension whose provider is unavailable/missing is **skipped with a
   warning** (never silently empties the result).
5. Returns `ScopeResolution`.

### Ownership & discovery

- **Employees module** ships providers for the 8 backed dimensions + the
  `IBasePopulationProvider`. Value sources: master-data lookups for catalog-backed
  dimensions; `StaticEnum` for `Status`.
- **Disabled dimensions** registered as `ScopeDimensionInfo { IsAvailable = false,
  UnavailableNote = "Backing provider not yet available" }` (static registration so the
  UI can list them).
- `services.AddScopeProvidersFromAssembly(assembly)` per module; `ScopeEngine` builds a
  `DimensionKey → provider` map (last-wins override — same pattern as
  `EffectExecutorRegistry`).

### Wiring into the run

`PayrollRunEngine.CreateAsync` calls `IScopeEngine.ResolveAsync(version.SelectionScope)`
and freezes the result into `PayrollRunPopulation`. `PayrollFactProvider` reads the
frozen population instead of re-querying `EmployeeFilterJson` live.

## Cutoff & calculation settings

- **Cutoff:** persisted typed columns; validation `1 ≤ CutoffDay ≤ 31`. Run-create
  screen shows a derived read-only message: *"This period closes on day {CutoffDay};
  transactions dated after it carry to the next period."* Carry-over filtering itself
  is sub-project 2/3.
- **Calc settings:** persisted + UI-edited. `DayBasis` threaded into the fact
  provider's daily-wage proration now: `CalendarMonth` → basic / actualDaysInMonth,
  `Fixed30` → basic / 30, `WorkingDays` → basic / workingDaysInMonth. Component-gating
  toggles read by the engine in sub-project 3.

## Config versioning operations

- **Clone** — copy a published version into a new `Draft` (next `VersionNumber`).
- **Publish** — `Draft → Published`; supersedes prior published version, sets
  `PublishedAt`, closes prior version's `EffectiveTo`. Immutable thereafter.
- **Simulate** — run a `Draft`/`IsSimulation` version through the existing
  `PayrollPreviewEngine` (no DB writes, no ledger) to validate scope + calc.

## API surface (extend `PayrollController`, prefix `api/payroll`)

Permissions: reads `Payroll.View`; config mutations new `Payroll.Configure`.

| Verb + Route | Purpose |
|---|---|
| `GET /types` | List types (header + active-version summary) |
| `POST /types` | Create type (header + initial draft version) |
| `GET /types/{id}` | Type detail + all versions |
| `PUT /types/{id}` | Edit header (name/category/status) |
| `POST /types/{id}/versions` | Create draft version |
| `POST /types/{id}/versions/{vid}/clone` | Clone → new draft |
| `PUT /types/{id}/versions/{vid}` | Edit draft (scope/calc/cutoff/payment/export/workflow/ruleset) |
| `POST /types/{id}/versions/{vid}/publish` | Publish draft |
| `POST /types/{id}/versions/{vid}/simulate` | Dry-run via preview engine |
| `GET /scope/dimensions` | Registry list (available + disabled-with-note) |
| `POST /scope/resolve` | Resolve a scope draft → counts + sample (live preview) |

## Frontend (Settings → Payroll → Payroll Types)

- **`/settings/payroll/types`** — list (code, AR/EN name, category badge, active
  version, status, active/inactive toggle); "New type". Gated `Payroll.View`; mutations
  `Payroll.Configure`.
- **`/settings/payroll/types/[id]`** — version timeline (VersionNumber, EffectiveFrom/
  To, status, Clone/Publish/Simulate) + tabbed version editor:
  - *General* — category, frequency, currency, payment-method scope, default export
    format, approval workflow, rule-set version.
  - *Selection Scope* — include/exclude builder from `GET /scope/dimensions`; values
    from each dimension's value source; disabled dimensions greyed with note; **live
    count** via `POST /scope/resolve`.
  - *Calculation* — day basis + toggles + excluded allowances.
  - *Cutoff* — cutoff day, closing/payment dates, carry-over toggle + preview message.
- Reuse `AccessGuard`, RTL, and the existing master-data UI for the new catalogs.

## Migration, seeding, tests

- **Migration `PayrollTypesAndScope`:** `CategoryId` on `engine_payroll_definitions`;
  typed + JSON columns on `engine_payroll_definition_versions`;
  `engine_payroll_run_population` table. Backfill `EmployeeFilterJson` →
  `SelectionScopeJson.include` (Department + employeeIds).
- **Seeding:** add `PayrollTypeCategory` + `PayrollExportFormat` rows to
  `MasterDataDefaults`; `StandardPayrollSeeder` stamps the `MONTHLY` type:
  `CategoryId = Regular`, `DefaultExportFormat = PDF`, `CutoffDay = 27`,
  `DayBasis = CalendarMonth`, default calc toggles.
- **Tests (xUnit):** scope resolution (OR within / AND across / exclude-wins /
  unavailable-dimension-skipped / mode=All); scope→population snapshot freeze; version
  Clone/Publish transitions + `EffectiveTo` closure; `DayBasis` proration; backfill
  migration correctness.

## Out of scope (later sub-projects)

- Additions/Deductions records & pages; attendance→deduction sync (sub-project 2).
- Cutoff carry-over enforcement; component-gating calc toggles; exclusion validity
  reasons; KPI/employee-table/excluded-section UI; lifecycle timeline (sub-project 3).
- Payslip PDFs (sub-project 4). Exports (sub-project 5).
- Activating disabled scope dimensions (requires each owning module to ship a provider
  + backing data).

# Sub-project 2A — Payroll Transaction Records, Lifecycle & Pages (Design)

**Status:** Approved design — ready for implementation planning.
**Parent overview:** `2026-06-30-payroll-subproject-2-additions-deductions-OVERVIEW.md`
**Master spec:** `2026-06-30-payroll-engine-redesign-master.md`
**Predecessor:** `2026-06-30-payroll-types-scope-cutoff-design.md` (sub-project 1 — done, deployed)

## Increment context

Sub-project 2 (Additions & Deductions + attendance-deduction sync) is sliced into three independently shippable increments:

- **2A (this spec)** — Records, lifecycle & pages. The data model, transaction lifecycle, manual workflow, and the two HR/Finance pages. No engine consumption yet.
- **2B** — Engine integration: run calc consumes approved records (respecting `CalcSettingsJson` toggles), cutoff carry-over enforcement + impact preview, attendance→deduction sync, retire/reconcile `ATTENDANCE_DED`, populate posting metadata + immutable locking.
- **2C** — Enterprise scale: batch Excel import (validate→preview→bulk create), duplicate/conflict detection, `IPayrollTransaction` abstraction (engine consumes sources via one interface), configurable transaction priority, richer attachment metadata.

This spec covers **2A only**. Columns and lifecycle states that 2B/2C depend on are *defined* here (so no second migration churns the same table), but their behavior is implemented in the later increment. Each cross-increment seam is called out explicitly below.

## Goal

Make every payroll addition and deduction a **visible, traceable record** that exists *before* payroll approval. Increment 2A delivers the record store, its lifecycle, and the UI to manage it — so HR/Finance can create, list, filter, edit, approve, attach to, and cancel/reverse additions and deductions. Records become visible immediately, even before the payroll engine reads them (which is 2B).

## Non-goals (2A)

- Engine/run consumption of records (2B).
- Live cutoff carry-over computation (2B). 2A stores an *intended* target period only.
- Attendance→deduction auto-creation (2B).
- Actually posting to the ledger / populating posting metadata (2B).
- Batch import, duplicate detection, `IPayrollTransaction` abstraction, transaction priority, rich attachment metadata (2C).

---

## 1. Data model

A **single unified entity** `PayrollTransaction` with a `Kind` discriminator (Addition vs Deduction), rather than two parallel entities. They share an identical shape and differ only by sign, which master-data catalog the type references, and their `SourceModule` values. The unified model gives one lifecycle, one state machine, one API (filtered by kind), and naturally becomes the `IPayrollTransaction` abstraction in 2C. The two pages are filtered views of the same store.

### Placement (follows the existing finance-engine layout)

- Entity → `backend/src/HR.Domain/Engines/Finance/Entities/PayrollTransaction.cs`
- Enums → alongside the entity (or in `HR.Domain/Engines/Finance/Entities/`): `PayrollTransactionKind`, `PayrollTransactionStatus`.
- State machine → `backend/src/HR.Domain/Engines/Finance/StateMachine/PayrollTransactionStateMachine.cs` (modeled on `PayrollRunStateMachine.cs`).
- EF config → a new `IEntityTypeConfiguration<PayrollTransaction>` class in `backend/src/HR.Infrastructure/Persistence/Configurations/Engines/FinanceConfigurations.cs` (auto-discovered via `ApplyConfigurationsFromAssembly`).
- DbSet → `backend/src/HR.Infrastructure/Persistence/ApplicationDbContext.cs` (near the other finance DbSets, ~line 259) + the `IApplicationDbContext` interface.
- Application interface/DTOs → `backend/src/HR.Application/Engines/Finance/`.
- Service/handlers → `backend/src/HR.Infrastructure/Engines/Finance/` (or a command/query handler set matching the module's existing style).
- Controller → extend `backend/src/HR.Modules/Payroll/Controllers/PayrollController.cs` (`api/payroll`).

### Base class

`PayrollTransaction : TenantEntity` — inherits `Id` (`BaseEntity`), audit fields `CreatedAt/By`, `UpdatedAt/By`, soft-delete `IsDeleted/DeletedAt/DeletedBy` (`AuditableEntity`), and `TenantId` (`TenantEntity`) with the global tenant query filter. Same base as `MasterDataItem`, `EmployeeAddition`, `AttendanceRecord`.

### Table: `engine_payroll_transactions`

| Field | Type | Notes |
|---|---|---|
| `Id` | Guid | from `BaseEntity` |
| `Kind` | `PayrollTransactionKind` (int) | `Addition=1`, `Deduction=2` |
| `EmployeeId` | Guid | FK → Employee |
| `TypeId` | Guid | → `MasterDataItem.Id`; the item's `ObjectType` must match the Kind (`AdditionType` for Addition, `DeductionType` for Deduction) |
| `Amount` | decimal(18,2) | **non-negative**; sign is implied by `Kind` (mirrors `FinancialLedgerEntry.Amount` which rejects negatives) |
| `TransactionDate` | DateTime (UTC) | the business-event date (when the bonus/penalty occurred) |
| `EffectiveDate` | DateTime (UTC) | drives payroll-period selection and cutoff; **all business calculation uses this** (enterprise req #14) |
| `TargetPeriodYear` | int? | intended payroll period (year), computed from `EffectiveDate` on create; display-only in 2A |
| `TargetPeriodMonth` | int? | intended payroll period (month), 1–12 |
| `IsRecurring` | bool | default false; **flag stored only in 2A** — per-period materialization is 2B |
| `RecurrenceEndDate` | DateTime? | optional end bound for recurring; stored only in 2A |
| `Notes` | string? | free text |
| `AttachmentFileId` | Guid? | → `StoredFile.Id`, served via `/api/files/{id}` (the `TerminationSettlement.DocumentFileId` pattern). Rich attachment metadata is 2C. |
| `SourceModule` | string | provenance; `"Manual"` in 2A. Future: Bonus/Commission/Overtime/Attendance/Loan/Penalty/Absence/Late. |
| `ReferenceType` | string? | traceability — entity type of the originating record |
| `ReferenceId` | Guid? | traceability — id of the originating record |
| `Status` | `PayrollTransactionStatus` (int) | lifecycle, default `Draft` (see §2) |
| `PayrollRunId` | Guid? | posting metadata — **column defined now, populated in 2B** |
| `PostedAt` | DateTime? | posting metadata — populated in 2B |
| `PostedBy` | Guid? | posting metadata — populated in 2B |
| `LedgerEntryId` | Guid? | → `FinancialLedgerEntry.Id`; posting metadata — populated in 2B |
| `ReversesTransactionId` | Guid? | self-FK; set when this row reverses a posted transaction — **defined now, transition wired in 2B** |
| `ReversalReason` | string? | reason captured on reversal |
| audit + `TenantId` | | from `TenantEntity` |

`CreatedAt` (from `AuditableEntity`) satisfies the third effective-dating date (`TransactionDate` / `EffectiveDate` / `CreatedAt`) — no separate column needed.

### Indexes (EF config)

- `(TenantId, EmployeeId)` — per-employee listing.
- `(TenantId, Kind, Status)` — page/status filtering.
- `(TenantId, TargetPeriodYear, TargetPeriodMonth)` — period filtering.
- `(ReferenceType, ReferenceId)` — source traceability (2B attendance sync).
- `ReversesTransactionId` — reversal lookups.

### Validation rules (enforced in the create/update handler)

- `Amount >= 0` (reject negatives; sign is by `Kind`).
- `TypeId` resolves to an active `MasterDataItem` whose `ObjectType` matches the `Kind` (`AdditionType`/`DeductionType`) within the tenant.
- `EmployeeId` resolves to an existing employee in the tenant.
- `EffectiveDate` required; `TransactionDate` defaults to `EffectiveDate` when omitted.
- `TargetPeriodYear`/`TargetPeriodMonth` derived from `EffectiveDate` on create (simple year/month of `EffectiveDate` in 2A; cutoff-aware derivation is 2B).
- Edits permitted only while `Status == Draft`.

---

## 2. Lifecycle & state machine

```
PayrollTransactionStatus {
  Draft, PendingApproval, Approved, Rejected, Cancelled, CarriedForward, Posted, Reversed
}
```

`PayrollTransactionStateMachine` mirrors `PayrollRunStateMachine`: a static transition table, `CanTransition`, `EnsureCanTransition` (throws `InvalidPayrollTransactionStateException`), and an `IsImmutable` helper (true once `Posted`).

### Transitions wired in 2A

| From | To | Trigger |
|---|---|---|
| Draft | PendingApproval | submit |
| Draft | Cancelled | cancel |
| PendingApproval | Approved | approve |
| PendingApproval | Rejected | reject (reason) |
| Rejected | Draft | re-open for edit |
| Approved | Cancelled | cancel before posting |

### Transitions defined now, exercised in 2B

| From | To | Trigger (2B) |
|---|---|---|
| Approved | Posted | payroll run execution posts the record |
| Approved | CarriedForward | effective date after cutoff → carried to next period |
| Posted | Reversed | reversal of a posted transaction |

### Immutability & editing rules

- **Editable only in `Draft`.** Any other state rejects `PUT`.
- `Approved` is locked except for `Cancelled` (2A) or `Posted`/`CarriedForward` (2B).
- `Posted` is permanently immutable — corrections only via reversal (enterprise reqs #3, #17). The reversal flow itself lands in 2B (since nothing is `Posted` until the engine runs).
- Soft-delete (`DELETE`) allowed only in `Draft`.

---

## 3. API surface

Extend `PayrollController` (`api/payroll`). All endpoints tenant-scoped and permission-gated on existing `Payroll.*` permissions (read on a View permission; create/edit/submit/cancel on a Manage permission; approve/reject on an Approve permission — exact permission codes confirmed against the existing payroll permission set during implementation).

| Method | Route | Purpose |
|---|---|---|
| GET | `/transactions` | list + filter: `kind`, `employeeId`, `periodYear`, `periodMonth`, `typeId`, `status`, `dateFrom`, `dateTo`; paged |
| GET | `/transactions/{id}` | single record (with type/employee display fields) |
| POST | `/transactions` | create (starts `Draft`; optional `submit=true` to go straight to `PendingApproval`) |
| PUT | `/transactions/{id}` | edit — **`Draft` only** |
| POST | `/transactions/{id}/submit` | Draft → PendingApproval |
| POST | `/transactions/{id}/approve` | PendingApproval → Approved |
| POST | `/transactions/{id}/reject` | PendingApproval → Rejected (reason in body) |
| POST | `/transactions/{id}/cancel` | Draft/Approved → Cancelled |
| POST | `/transactions/{id}/attachment` | upload file → `StoredFile`, set `AttachmentFileId` |
| DELETE | `/transactions/{id}` | soft-delete — **`Draft` only** |

Type dropdowns for the pages reuse the **existing master-data lookup endpoints** (`AdditionType` / `DeductionType` via the master-data query API) — no new catalog endpoint is introduced.

DTOs live in `backend/src/HR.Modules/Payroll/DTOs/` (request/response) following the controller's existing DTO style; list responses include resolved type name + employee name for display.

---

## 4. Frontend

Two routes — `/payroll/additions` and `/payroll/deductions` — render **one shared component** with the `Kind` preset, so they are filtered views over the same store. Built on the existing Next.js api-client/auth pattern with `usePermissions` gating (matching the established module conventions; read the relevant guide in `node_modules/next/dist/docs/` before writing Next.js code per AGENTS.md).

Each page provides:

- **Filterable table** — columns: employee, type, amount, effective date, target period, source, status badge. Filters: employee, period (year/month), type, status, date range.
- **Create / edit dialog** — employee picker, type dropdown (master-data filtered by kind), amount, transaction date, effective date, recurring toggle (+ optional end date), notes, attachment upload. Edit disabled unless `Draft`.
- **Row actions** — submit, approve, reject (reason prompt), cancel, view/replace attachment — each shown/enabled per current status and permission.
- **Impact hint** — a "**Will affect {TargetPeriodMonth} {TargetPeriodYear}**" line derived from `EffectiveDate`. (The cutoff-aware "carried to next period because created after cutoff" reasoning is 2B; 2A shows the plain target period.)
- **Status badges** — color-coded across the full lifecycle enum.

---

## 5. Testing

Test-driven, following the repo's existing xUnit conventions and test-project layout.

- **State machine:** every legal transition succeeds; representative illegal transitions throw `InvalidPayrollTransactionStateException`; `IsImmutable` correct for `Posted`.
- **Create/update handler:** validation — `Amount >= 0`, `TypeId` ObjectType matches `Kind`, employee exists, `TransactionDate` defaults to `EffectiveDate`, target period derived from `EffectiveDate`; edit rejected when not `Draft`.
- **Workflow handlers:** submit/approve/reject/cancel drive the correct transitions and reject from illegal states.
- **Tenant scoping:** records are isolated per tenant (query filter honored).
- **Attachment:** upload creates a `StoredFile` and sets `AttachmentFileId`.

---

## 6. Migration & deployment

- Create migration `PayrollTransactions` from `backend/src/HR.Infrastructure`:
  `dotnet ef migrations add PayrollTransactions --startup-project ../HR.Api`
  Adds `engine_payroll_transactions` + the indexes in §1. No changes to existing tables.
- Local `appsettings.json` stays on `localhost` (do not commit prod secrets — per CLAUDE.md). Apply to Azure via the standard `dotnet ef database update` against the injected production connection string, following the established repo workflow.
- Redeploy the API; Vercel auto-deploys the frontend.

---

## 7. Cross-increment seams (explicit)

| Defined in 2A | Consumed/implemented in |
|---|---|
| `PayrollRunId`/`PostedAt`/`PostedBy`/`LedgerEntryId` columns | 2B — populated at run execution; posts to `IFinancialLedger` |
| `Approved→Posted`, `Approved→CarriedForward`, `Posted→Reversed` transitions | 2B — run execution, cutoff carry, reversal flow |
| `ReversesTransactionId`/`ReversalReason` | 2B — reversal endpoint + immutability enforcement |
| `IsRecurring`/`RecurrenceEndDate` | 2B — per-period materialization |
| `EffectiveDate` + `TargetPeriod*` | 2B — cutoff-aware period derivation honoring `PayrollDefinitionVersion.CutoffDay`/`CarryToNextPeriod` |
| `SourceModule`/`ReferenceType`/`ReferenceId` | 2B — attendance sync sets `Attendance` + `AttendanceRecord` reference; `ATTENDANCE_DED` reconciliation |
| Single `PayrollTransaction` entity | 2C — generalized behind `IPayrollTransaction`; priority + duplicate detection layered on |

## Acceptance (2A slice)

- A created addition/deduction appears in `/payroll/additions` or `/payroll/deductions` immediately, with its type, amount, effective date, target period, source, and status.
- Additions and deductions are managed as one store but shown on separate pages.
- The full lifecycle is enforced: only `Draft` records are editable; submit/approve/reject/cancel follow the state machine; illegal transitions are rejected.
- No deduction can exist without a traceable record (every row carries source + optional reference + audit).
- Attachments upload to the file store and link by `AttachmentFileId`.
- Migration applies cleanly to Azure; API + frontend deploy.

# Sub-project 2D — Attendance → Deduction Records (Design)

**Status:** Approved design — ready for implementation planning.
**Date:** 2026-07-02
**Parent:** `2026-06-30-payroll-engine-redesign-master.md`
**Overview:** `2026-06-30-payroll-subproject-2-additions-deductions-OVERVIEW.md` (§3 attendance sync, §19/§20 traceability)
**Predecessor:** `2026-07-01-payroll-subproject-2c-consumption-posting-reversal-design.md` (2C — consumption/posting/reversal spine, shipped & deployed, merged to main via PR #11)
**Branch:** `feat/financial-engine`

> Built via the standard **brainstorm → spec → writing-plans → subagent build** cycle (same as 2A/2C).

---

## 1. Goal

Deliver the core "no hidden deductions" philosophy change from the sub-project 2 overview: **attendance penalties become visible, traceable `PayrollTransaction` deduction records** instead of an aggregate computed silently inside a payroll rule. Absence, late, and shortage penalties appear in `/payroll/deductions` **before** payroll approval, each with a drill-down to the exact attendance records that produced it, and each flows through 2C's existing consume → post → reverse spine.

This retires the seeded `ATTENDANCE_DED` rule and makes attendance-sourced records the **single source of truth** for attendance deductions.

## 2. Background — the current gap

- Attendance deductions today are a **computed rule output**, never a record. `StandardPayrollSeeder` seeds an `ATTENDANCE_DED` deduction rule with expression `ROUND(AbsentDays * DailyWage + (LateHours + ShortageHours) * HourlyWage, 2)` (`StandardPayrollSeeder.cs:37-38`).
- Its facts are computed by **live aggregation of `AttendanceRecord` rows** in `PayrollFactProvider.BuildInputsAsync` (`PayrollFactProvider.cs:85-97`, wage math `125-131`): `AbsentDays`, `LateHours`, `ShortageHours`, `DailyWage`, `HourlyWage`.
- Nothing creates a deduction when a penalty is applied — `AttendanceService.RecalcAsync` (`AttendanceService.cs:286-311`) only writes attendance rows + audit logs. The penalty stays implicit until a run reads it. This is the "hidden deduction" the overview targets.
- **2C already built the record→run spine:** `PayrollTransactionConsumer.GetConsumableAsync` consumes **all** `Approved` transactions regardless of `SourceModule` (`PayrollTransactionConsumer.cs:25-27`), merges them additively (`PayrollComputation.cs:86-94`), posts one ledger entry each on Execute, and supports reversal. The `PayrollTransaction` entity already carries `SourceModule` / `ReferenceType` / `ReferenceId` columns (`PayrollTransaction.cs:46,49,50`).
- **The only missing pieces:** (a) something that *creates* attendance-sourced records, (b) retiring the now-duplicate rule, and (c) a proper drill-down reference.

## 3. Scope

In scope:
1. **`AttendanceDeductionSyncService`** — materializes per-employee, per-period, per-penalty-type `Approved` deduction records from attendance data, idempotently.
2. **Two triggers (A+C hybrid):** guaranteed materialization at **Calculate**, plus an on-demand **"Sync Now"** endpoint for early HR review.
3. **Retire `ATTENDANCE_DED`** — remove from the seeder, neutralize on existing tenants, make records the sole source.
4. **Drill-down reference table** + breakdown drawer UI.
5. **Sync report** returned from the service and surfaced in the UI.
6. **Regression tests** proving no double-counting.

Explicitly **out of scope** (later sub-projects / adjacent work):
- **Manual-transaction run-state guard (diagnosis bug #4)** — creating/approving a *manual* transaction against an already-closed period is a separate stabilization item; a small follow-up, not 2D. (2D's own sync path is inherently run-state-aware: it skips Posted records.)
- Duplicate/conflict detection (overview §9), `IPayrollTransaction` abstraction + priority (§11/§12), batch Excel import (§16), attachment metadata (§15) — future sub-projects.
- Overtime/loan/expense as records — 2D covers **attendance penalties only**.
- Post-posting corrections use **2C's existing reversal model** unchanged; 2D adds no new reversal logic.

## 4. Architecture

One new materialization service, one shared wage calculator extracted from the fact provider, one reference table, plus retirement of the rule. Everything downstream (consume/post/reverse) is 2C, unchanged.

```
              ┌─ Sync Now (HR, POST /attendance-deductions/sync) ─┐
              │                                                    │
Attendance ──►│  AttendanceDeductionSyncService.SyncAsync         │──► Approved PayrollTransaction
records       │   • AttendanceWageCalculator (shared w/ facts)    │    (SourceModule="Attendance",
              │   • per (Employee, Period, PenaltyKind) upsert    │     one per penalty kind)
              └─ Calculate-time call (guaranteed, pre-consume) ───┘         │
                                                                            ▼
                                              2C spine: Consume ─ Post ─ Reverse (UNCHANGED)
                                                                            │
   PayrollTransactionAttendanceReference (drill-down snapshot) ────────────┘ audit chain
```

### 4.1 `AttendancePenaltyKind` enum (business semantics)

New enum `AttendancePenaltyKind { Absence, Late, Shortage }` in `HR.Domain`. **Engine logic keys on this enum**, never on master-data labels. Each kind maps to a customer-configurable `DeductionType` master-data item **by code** (`ABSENCE`, `LATE`, `SHORTAGE`), resolved at sync time. This separates business meaning (fixed) from presentation (configurable labels/UI).

### 4.2 `AttendanceWageCalculator` (shared, extracted)

Extract the attendance aggregation + `DailyWage`/`HourlyWage` computation currently inline in `PayrollFactProvider.BuildInputsAsync` (`:85-97,125-131`) into a reusable `AttendanceWageCalculator` in `HR.Infrastructure/Engines/Finance`. It returns, per employee for a period: `AbsentDays`, `LateHours`, `ShortageHours`, `DailyWage`, `HourlyWage`, and the contributing `AttendanceRecord` rows (id, date, minutes, status). **Both** `PayrollFactProvider` and `AttendanceDeductionSyncService` call it, so the wage formula has exactly one definition — no drift between the (inert) facts and the records.

### 4.3 `AttendanceDeductionSyncService` (new, `HR.Infrastructure/Engines/Finance`)

`Task<AttendanceDeductionSyncReport> SyncAsync(PayrollPeriod period, IReadOnlyCollection<Guid> employeeIds, string actor, CancellationToken ct)`

Per employee, per `AttendancePenaltyKind`:
- Compute the amount from `AttendanceWageCalculator`:
  - `Absence  = AbsentDays  × DailyWage`
  - `Late     = LateHours   × HourlyWage`
  - `Shortage = ShortageHours × HourlyWage`
  (identical to the retired rule's math, so amounts are unchanged.)
- **Idempotent upsert** keyed on `(EmployeeId, TargetPeriodYear, TargetPeriodMonth, AttendancePenaltyKind, SourceModule="Attendance")`:
  - **Create** (born `Approved`) if none exists and amount > 0.
  - **Update** the amount + reference snapshot if a matching record exists **and is not yet `Posted`**.
  - **Remove** (transition to `Cancelled`) a matching non-posted record whose amount is now 0 (a correction cleared the penalty) — no stale deduction stranded.
  - **Skip** (leave untouched, count as `SkippedPosted`) any matching record already `Posted` — post-posting changes require 2C reversal / next-period adjustment.
- On create/update, populate provenance: `SourceModule="Attendance"`, `ReferenceType="AttendancePeriodPenalty"`, `TypeId` = the `DeductionType` resolved from the kind's code, and rewrite the drill-down rows in `PayrollTransactionAttendanceReference` (§4.4).
- Respects `CalcSettingsJson.includeAttendanceDeductions`: when false, `SyncAsync` is a no-op returning an empty report.
- Records are **born `Approved`** (per the approval decision — attendance is the source of truth; HR corrects attendance, not the deduction). `UpdateAsync`'s `IsEditable`(Draft-only) guard is bypassed deliberately: the sync service mutates the amount of its own non-posted `Approved` records directly, because attendance is authoritative until posting.

**`CreatePayrollTransactionArgs` gap:** `PayrollTransactionService.CreateAsync` currently hardcodes `SourceModule="Manual"` and omits `ReferenceType`/`ReferenceId` (`PayrollTransactionService.cs:51`). The sync service does **not** go through that manual path; it writes attendance records with full provenance itself (or via a new internal create overload that accepts source/reference), keeping the manual API untouched.

### 4.4 `PayrollTransactionAttendanceReference` (new entity + migration)

A dedicated reference table (not JSON in `Notes`) for efficient audit/reporting/drill-down:

| Column | Purpose |
|---|---|
| `Id` | PK |
| `PayrollTransactionId` | FK → the attendance deduction record |
| `AttendanceRecordId` | FK → the contributing `AttendanceRecord` |
| `Date` | snapshot of the attendance date |
| `PenaltyKind` | `AttendancePenaltyKind` |
| `Minutes` / `Days` | snapshot of the contributing quantity (late/shortage minutes, or 1 absent day) |
| `AmountContribution` | snapshot of this row's share of the deduction |
| tenant/audit fields | standard |

Snapshot columns mean the breakdown drawer and audit remain **historically accurate even if the underlying attendance later changes** (overview §19 — never overwrite original business information). On re-sync of a non-posted record, its reference rows are rewritten to match the new calculation; once posted, they are frozen.

**Migration:** 2D adds one EF migration for this table (the first schema change in the sub-project 2 series). The `PayrollTransaction` provenance columns already exist (2A) and need no change.

### 4.5 `AttendanceDeductionSyncReport`

Returned from `SyncAsync` and surfaced in the UI:

```
{ Created, Updated, Removed, SkippedPosted, TotalProcessed }
```

`TotalProcessed` = employees × kinds evaluated. Calculate-time also logs this report.

### 4.6 Retiring `ATTENDANCE_DED`

- **New tenants:** remove the `ATTENDANCE_DED` entry from `StandardPayrollSeeder.Rules[]` (`:37-38`).
- **Existing tenants:** add an idempotent neutralize step in `EnsureRulesPresentAsync` (`StandardPayrollSeeder.cs:121-135`) that **deactivates** any existing `ATTENDANCE_DED` rule row (data change, no migration). Deactivation (not deletion) preserves historical run auditability.
- **Facts stay, inert:** `PayrollFactProvider` continues to emit `AbsentDays`/`LateHours`/`ShortageHours` (no rule reads them now; available for reporting). Only the *rule* dies — nothing else that references those facts breaks.
- **Master-data types:** seed `LATE` and `SHORTAGE` `DeductionType` items (alongside the existing `ABSENCE`) so the three kinds render as distinct, clearly-labeled payslip lines.

### 4.7 Two call sites (A+C hybrid)

1. **Calculate-time (guaranteed):** `PayrollRunEngine.CalculateAsync` invokes `SyncAsync` for the run population **before** `PayrollTransactionConsumer` runs, so `Approved` attendance records always exist → are consumed → are posted. This is the guarantee that replaces the always-on rule.
2. **Sync Now (early review):** `POST /api/payroll/attendance-deductions/sync` (gated `Payroll.Configure`), body `{ year, month, employeeIds? }`, returns the sync report. Lets HR materialize and eyeball deductions days before running payroll.

## 5. Frontend

- **`/payroll/deductions`:** attendance-sourced records (`SourceModule="Attendance"`) render with a **breakdown drawer** — a table of contributing dates, minutes/hours or absent days, per-row amount, and total — sourced from `PayrollTransactionAttendanceReference`. A **"Sync attendance deductions"** button (period selector) calls the sync endpoint and shows the returned report (`Created/Updated/Removed/Skipped/Total`) as a toast/summary.
- Attendance-sourced records are **read-only** in the deductions UI (born `Approved`, source=Attendance). The edit affordance is replaced with guidance: *"Attendance-driven — correct the attendance record and re-sync."* Post-posting fixes route to the 2C reverse action.
- **Run details** (2C) already renders per-record lines by type; attendance deductions appear there as Absence/Late/Shortage lines automatically.

## 6. State, error handling & audit

- Records use `PayrollTransactionStateMachine` unchanged: born `Approved`, → `Posted` on Execute (2C), → `Reversed` via 2C reversal. Zero-cleared non-posted records go to `Cancelled`.
- Business-rule failures throw `DomainException` (422): e.g. "No DeductionType configured for penalty kind 'Late' (code LATE)." Missing master-data type is a config error, surfaced clearly, not a silent skip.
- **Audit chain (overview §20):** `AttendanceRecord ids` → `PayrollTransactionAttendanceReference` → `PayrollTransaction` (source=Attendance) → `PayrollRunId`/`PostedAt`/`PostedBy` (2C posting) → `LedgerEntryId` (2C). Every link is a real FK/column; the drawer walks it end to end.

## 7. Testing

xUnit in `backend/tests/HR.Domain.Finance.Tests` (Npgsql-sensitive paths noted — InMemory won't catch timestamptz issues):

1. **No double-count** — an employee with absence + late + shortage yields exactly one `Approved` deduction per kind, correct amounts, and the retired `ATTENDANCE_DED` rule contributes **nothing** to the run.
2. **Idempotent re-sync** — running `SyncAsync` twice produces the same records/amounts/reference rows (upsert, not duplicates); report shows `Created` then `Updated`.
3. **Refresh before post** — attendance changes → re-sync updates the non-posted record's amount and rewrites its reference rows; a **`Posted`** record is untouched and counted `SkippedPosted`.
4. **Guarantee at Calculate** — a run whose Sync Now was never clicked still materializes `Approved` attendance records before consumption, and they post to the ledger.
5. **Zero-clear** — a correction that removes a penalty transitions the stale non-posted record to `Cancelled`; it is not consumed.
6. **Neutralize** — a tenant seeded with a live `ATTENDANCE_DED` rule has it deactivated on seed-refresh; no attendance line originates from the rule afterward.
7. **Enum/label separation** — renaming the `LATE` `DeductionType` label does not change which records the engine produces or their upsert identity.
8. **Toggle** — `includeAttendanceDeductions=false` makes `SyncAsync` a no-op (empty report, no records).

## 8. Migration & open implementation items

- **One migration** — `PayrollTransactionAttendanceReference` table. No change to `PayrollTransaction` (2A columns suffice) or the ledger (reuses 2C's `ReferenceType`/`ReferenceId`).
- **Seed additions** — `LATE` + `SHORTAGE` `DeductionType` master-data items (idempotent, top-upped like existing types).
- **Rule neutralization** — data-level deactivation of `ATTENDANCE_DED` on existing tenants via `EnsureRulesPresentAsync`; no migration.
- **Shared calculator refactor** — extracting `AttendanceWageCalculator` changes `PayrollFactProvider` internals only; the emitted fact bag is unchanged (regression-guarded).

## 9. Acceptance criteria

- Applying an attendance penalty and running Sync Now (or Calculate) produces an `Approved` `PayrollTransaction` per penalty kind in `/payroll/deductions`, source `Attendance`, **before** approval, each with a working breakdown drawer.
- Additions/deductions still show separately by type on run details; attendance deductions appear as Absence/Late/Shortage lines.
- No employee is deducted twice — the `ATTENDANCE_DED` rule no longer contributes once records exist, on both new and existing tenants.
- Re-syncing is idempotent; changing attendance before posting refreshes the record; a posted record is immutable and only correctable via 2C reversal.
- The sync report (Created/Updated/Removed/Skipped/Total) is returned by the service and shown in the UI.
- Full audit chain resolves: attendance record → reference → transaction → run → ledger entry.

# Sub-project 2E — Attendance Daily Actions + Overtime + Configurable Rates + Excuse Trigger (Design)

**Status:** Approved design — ready for implementation planning.
**Date:** 2026-07-02
**Parent:** `2026-06-30-payroll-engine-redesign-master.md`
**Roadmap:** `2026-07-02-payroll-run-operations-enhancement-ROADMAP.md` (Areas 4, 5; parts of 7, 8)
**Predecessor:** `2026-07-02-payroll-subproject-2d-attendance-deduction-records-design.md` (2D — attendance→deduction records, shipped & deployed)
**Branch:** `feat/financial-engine`

> Built via the standard **brainstorm → spec → writing-plans → subagent build** cycle. 2E is an incremental extension of the live 2D engine.

---

## 1. Goal

Complete the "no hidden payroll effects" attendance→payroll story that 2D began:
1. **Overtime becomes a visible, paid `PayrollTransaction` Addition** (2D covered only deductions; overtime is currently unpaid).
2. **All attendance calculation rates are configurable** (absence/late/shortage/overtime multipliers), not hard-coded.
3. **Daily attendance page actions** let HR materialize an employee's attendance payroll impact for a chosen month, on demand.
4. **Approved excuses/leave genuinely cancel the related deduction** by fixing the attendance record at its source, so 2D's cancel-on-zero fires.

## 2. Background — what 2D shipped and where the gaps are

- `AttendanceWageCalculator` already computes per-employee `AbsentDays`/`LateMinutes`/`ShortageMinutes`/**`OvertimeMinutes`** and per-day `BreakdownRowsAsync` rows (`AttendanceWageCalculator.cs`).
- `AttendanceDeductionSyncService` materializes per-employee/period/kind **Approved** `PayrollTransaction` **deduction** records (absence/late/shortage), idempotent upsert, **cancel-on-zero**, `PayrollTransactionAttendanceReference` drill-down (`AttendanceDeductionSyncService.cs`). Guaranteed at `PayrollRunEngine.CalculateAsync`; on-demand via `POST /api/payroll/attendance-deductions/sync` (`Payroll.Configure`).
- `PayrollCalcSettings.IncludeAttendanceDeductions(json)` reads a single toggle.
- **Gaps:** (a) overtime unpaid — no rule reads `OvertimeHours`, no addition record; (b) deduction amounts are implicitly `1.0×` (no rate config); (c) no per-employee daily entry point on the attendance page; (d) approved excuse/leave leaves penalty minutes stale so cancel-on-zero never fires (`AttendanceCorrectionExecutor` sets `Status=Present` but keeps `LateMinutes`/`ShortageMinutes`; `AttendanceApplyLeaveDaysExecutor` **inserts a duplicate** OnLeave row instead of updating the penalty row).

## 3. Scope

In scope:
1. Generalize the 2D sync engine to a 4th kind — **Overtime (Addition)** — reusing all upsert/cancel/reference machinery.
2. **Configurable rates** via `CalcSettingsJson.attendanceRates` (defaults absence/late/shortage `1.0`, overtime `1.5`).
3. **Overtime is opt-in** (`includeOvertime` default **false**) — deliberate, unlike automatic absence deductions.
4. **Daily attendance page** per-employee actions + a scoped sync endpoint gated by a new `Attendance.PayrollImpact.Create` permission.
5. **Excuse/leave trigger fix** at the attendance source so cancel-on-zero fires.
6. **Permission** `Attendance.PayrollImpact.Create` + audit origin marker.

Explicitly **out of scope:** post-posting corrections (reversal — sub-project 6); per-day (vs per-period) record shape (rejected in brainstorming — daily action reuses 2D's per-employee/period model); payslip rendering (sub-project 4); a full calc-settings settings UI (2E adds the config *reading* + values pass through the existing version-update `CalcSettingsJson` field; a dedicated settings screen is later).

## 4. Architecture

### 4.1 Rename + generalize the sync engine
- Rename `AttendanceDeductionSyncService`/`IAttendanceDeductionSyncService` → **`AttendancePayrollSyncService`/`IAttendancePayrollSyncService`**; `AttendancePenaltyKind` → **`AttendancePayrollKind { Absence = 1, Late = 2, Shortage = 3, Overtime = 4 }`** (values 1–3 unchanged → **no data migration**; the `PayrollTransactionAttendanceReference.PenaltyKind` int column is stable). Update DI, `PayrollRunEngine`, `PayrollController`, tests.
- `ResolveTypesAsync` returns, per kind, both the master-data `TypeId` **and** the `PayrollTransactionKind`:
  - Absence/Late/Shortage → `DeductionType` (codes `ABSENCE`/`LATE`/`SHORTAGE`), `Kind=Deduction`.
  - **Overtime → `AdditionType` code `OVERTIME` (already seeded), `Kind=Addition`.**
- The per-kind loop computes `amount = quantity × rate × multiplier` and creates the transaction with the resolved `Kind`. Everything else (upsert key `(EmployeeId, TargetPeriod, TypeId, SourceModule="Attendance")`, born `Approved`, cancel-on-zero, skip Posted/Reversed, reference rows) is unchanged from 2D.
- **No double-count / no new payroll rule:** the overtime addition is a `Kind=Addition` `PayrollTransaction`, consumed by the source-agnostic 2C consumer and summed by the existing `ADDITIONS` rule — exactly like a manual bonus. `OvertimeHours` stays an inert fact.

### 4.2 Configurable rates — extend `PayrollCalcSettings`
Add a typed reader over `CalcSettingsJson`:
```json
{
  "includeAttendanceDeductions": true,
  "includeOvertime": false,
  "attendanceRates": { "absenceMultiplier": 1.0, "lateMultiplier": 1.0, "shortageMultiplier": 1.0, "overtimeMultiplier": 1.5 }
}
```
- `PayrollCalcSettings.AttendanceRates(json)` → a struct with the four multipliers, each defaulting (1.0/1.0/1.0/1.5) when the key/JSON is absent or malformed (mirrors the existing default-on toggle behaviour).
- `PayrollCalcSettings.IncludeOvertime(json)` → default **false**.
- Amounts: `Absence = AbsentDays × DailyWage × absenceMultiplier`; `Late = LateHours × HourlyWage × lateMultiplier`; `Shortage = ShortageHours × HourlyWage × shortageMultiplier`; `Overtime = OvertimeHours × HourlyWage × overtimeMultiplier`. (2D's deductions were implicitly `×1.0`; defaults preserve current behaviour exactly.)
- The sync service evaluates the Overtime kind only when `IncludeOvertime` is true (or when explicitly requested via the daily action — see 4.4). Deduction kinds continue to obey `IncludeAttendanceDeductions`.

### 4.3 Wage basis for overtime
`HourlyWage = DailyWage / 8` — the exact value the fact provider already computes and the sync service already reads from the fact bag (zero drift, same source as 2D deductions). No new wage math.

### 4.4 Daily attendance actions + scoped endpoint
- **Endpoint:** `POST /api/attendance/payroll-impact/sync` (new; gated `Attendance.PayrollImpact.Create`), body `{ employeeId: Guid, year: int, month: int, includeOvertime?: bool }`. It resolves the standard payroll definition's current version, then calls `AttendancePayrollSyncService.SyncAsync(version, PayrollPeriod.Monthly(year,month), [employeeId], ...)`. When the action is the explicit "Calculate overtime addition" button, it passes `includeOvertime=true` so overtime materializes even if the version's config default is false. Returns the sync report.
- The endpoint lives on an **Attendance** controller (permission family + triggered from the attendance page). It depends on `IAttendancePayrollSyncService` — the same engine the payroll run uses. Placing it here keeps the attendance page decoupled from payroll internals via the interface.
- **Frontend:** in `attendance/page.tsx` `DailyTable`, add a per-row action group (gated by `Attendance.PayrollImpact.Create`) with a target-month selector (default = the viewed month): *Calculate absence / late / shortage deduction · Calculate overtime addition*. All four call the endpoint for that employee/month; the deduction buttons run the deduction sync, the overtime button passes `includeOvertime=true`. On success, a toast summarises the sync report (reuse 2D's report shape). Records surface on `/payroll/deductions` and `/payroll/additions` (shared `TransactionsPage`).
- **Granularity note (decided):** the daily row is the *entry point*; the materialised record is the per-employee/period aggregate (2D model). The breakdown drawer already shows the per-day detail, so no day-level visibility is lost and no competing per-day record shape is introduced.

### 4.5 Excuse/leave trigger — fix at the source
- **`AttendanceCorrectionExecutor`** — on both the insert and the existing-record branch, when setting `Status = Present`, also set `LateMinutes = 0`, `ShortageMinutes = 0` (an excused day carries no penalty). (Overtime on an excused day is out of scope — leave `OvertimeMinutes` untouched.)
- **`AttendanceApplyLeaveDaysExecutor`** — change from blind INSERT to **upsert**: if a record already exists for the day, update it to `Status = OnLeave` and zero `LateMinutes`/`ShortageMinutes` (instead of adding a duplicate row that leaves the penalty intact). Only insert when no record exists.
- **Result:** the attendance record no longer shows a penalty → `AttendanceWageCalculator` aggregates zero for that day → on the next sync (guaranteed at Calculate, pre-posting) 2D's **cancel-on-zero** transitions the stale deduction to `Cancelled`. No new deduction-cancellation code.
- **Optional eager re-sync:** the executors run inside the completion transaction with the shared `ApplicationDbContext`; a targeted re-sync there would cancel the deduction immediately (so HR sees it before Calculate). Kept optional — the Calculate guarantee already satisfies "before posting"; include only if low-risk.

### 4.6 Audit / origin
The sync already stamps `SourceModule="Attendance"`, `ReferenceType`, and the reference rows. Add an **origin** marker distinguishing the daily-page action from the run/period sync (e.g. a `StatusReason`/audit note like `"AttendanceDaily"` vs `"PeriodSync"`), so Area 8's "from which screen" is answerable. No schema change required (reuse `StatusReason`/audit log).

## 5. State & compatibility

- `PayrollTransactionStateMachine` unchanged: attendance impacts born `Approved`, `→ Posted` on Execute (2C), `→ Cancelled` on cancel-on-zero, `→ Reversed` via reversal (2C). Overtime additions follow the identical lifecycle.
- Immutable-run model preserved; overtime additions are consumed at Calculate like any Approved transaction; posted once on Execute.
- Defaults preserve current behaviour: `includeOvertime=false` and rate multipliers defaulting to current effective values mean an existing tenant sees **no change** until it opts in / adjusts rates.

## 6. Error handling

- Missing `OVERTIME` `AdditionType` (or `ABSENCE`/`LATE`/`SHORTAGE`) → `DomainException` (422), same as 2D.
- Daily endpoint: unknown employee / no published payroll version → `DomainException`/`NotFoundException` per existing controller patterns.
- Rate config malformed → defaults applied (never throws), matching 2D's toggle behaviour.

## 7. Testing (xUnit `HR.Domain.Finance.Tests`, EF InMemory; + attendance module tests)

1. **Overtime addition** — an employee with overtime minutes and `includeOvertime=true` gets exactly one `Approved` `Kind=Addition` `PayrollTransaction` on the `OVERTIME` `AdditionType`, amount `= OvertimeHours × HourlyWage × 1.5`.
2. **Rate config** — non-default multipliers change the amounts proportionally; absent config = current 1.0×/1.5× defaults (regression: 2D deduction amounts unchanged).
3. **Overtime opt-out** — `includeOvertime=false` (default) produces no overtime record at Calculate.
4. **Overtime cancel-on-zero** — overtime minutes removed → re-sync cancels the addition.
5. **Excuse zeroes penalty** — `AttendanceCorrectionExecutor` sets `Status=Present` and zeroes minutes → re-sync cancels the deduction.
6. **Leave upsert** — `AttendanceApplyLeaveDaysExecutor` updates the existing day's record (no duplicate row) and zeroes minutes → deduction cancels.
7. **Daily endpoint scope** — the endpoint materialises only the requested employee/month and honours the explicit `includeOvertime=true` override.
8. **Rename regression** — the full finance suite stays green after the service/enum rename.

## 8. Migration & open implementation items

- **No new migration** — the `PayrollTransactionAttendanceReference` table + `PayrollTransaction` provenance columns (2D/2A) suffice; `AttendancePayrollKind.Overtime = 4` is a new int enum value, not a schema change; `CalcSettingsJson` already exists (jsonb).
- Rename touchpoints: service + interface + enum + DI registration + `PayrollRunEngine` field/ctor + `PayrollController` field/ctor + 2D tests.
- New permission `Attendance.PayrollImpact.Create` seeded in the permission seeder + granted to the appropriate default roles.
- New endpoint on the Attendance controller depending on `IAttendancePayrollSyncService`.

## 9. Acceptance criteria

- An employee with recorded overtime, after HR clicks "Calculate overtime addition" (or `includeOvertime=true`), has a visible `Approved` overtime **Addition** on `/payroll/additions`, paid through the run at `1.5×` (or the configured multiplier).
- Changing `attendanceRates` in the version's calc settings changes attendance amounts; defaults reproduce 2D exactly.
- A daily-page action materialises only that employee/month's impact and surfaces it on the Additions/Deductions pages.
- Approving an excuse (correction→Present) or leave for a penalised day removes the penalty at the source and the related deduction is `Cancelled` on the next sync (before posting).
- No overtime is paid, and no attendance amount changes, for a tenant that has not opted in / changed rates.

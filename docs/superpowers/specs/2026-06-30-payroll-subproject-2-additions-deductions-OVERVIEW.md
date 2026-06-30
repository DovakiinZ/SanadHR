# Sub-project 2 — Additions & Deductions + Attendance-Deduction Sync (Overview)

**Status:** Draft overview / scoping note — NOT a finalized spec.
**Parent:** `2026-06-30-payroll-engine-redesign-master.md`
**Predecessor:** `2026-06-30-payroll-types-scope-cutoff-design.md` (sub-project 1 — done, deployed)

> Before implementation, run the full **brainstorm → spec → writing-plans → subagent-driven build** cycle (same as sub-project 1). This file is just the starting scope.

## Goal

Make every payroll addition and deduction a **visible, traceable record** that exists *before* payroll approval — replacing the current fact-only model where attendance impacts are invisible inputs computed inside a rule. Attendance penalties become real deduction records with a source and reference. This is the spec's core philosophy change (no hidden deductions).

## Why now

Sub-project 1 shipped the **cutoff configuration** (`CutoffDay`, `CarryToNextPeriod`) but does not yet *enforce* carry-over because dated transactions didn't exist. This sub-project introduces those dated transactions, so cutoff enforcement is built here.

## Scope

### 1. Record-based entities (new)
- **`PayrollAddition`** — `Id`, `EmployeeId`, `AdditionTypeId` (master-data), `Amount`, `Date`, `PayrollPeriod` (year/month), `IsRecurring`, `Notes`, `AttachmentFileId?`, `ApprovalStatus`, `SourceModule` (Manual/Bonus/Commission/Overtime/…), `ReferenceType`/`ReferenceId?`, audit/tenant fields.
- **`PayrollDeduction`** — same shape, `DeductionTypeId` (master-data), plus `SourceModule` (Manual/Attendance/Loan/Penalty/Absence/Late/…) and `ReferenceType`/`ReferenceId` for traceability.
- Reuse existing master-data `AdditionType` / `DeductionType` catalogs (no duplicate field definitions).

### 2. Dedicated pages
- `/payroll/additions` and `/payroll/deductions` — HR/Finance create, list, filter (by employee/period/type/status), edit, attach files, approve. Every record visible before payroll approval.

### 3. Attendance → Deduction sync (the philosophy change)
- When HR applies an attendance penalty (absence/late/shortage), the attendance module creates a **`PayrollDeduction`** record with `SourceModule = Attendance` and `ReferenceType/ReferenceId = AttendanceRecordId` (or a new `AttendancePenaltyId`).
- Integrate via the existing **Completion Effects Engine** pattern (an `IEffectExecutor`, e.g. `Payroll.CreateDeduction`) and/or a direct attendance-service hook — decide during brainstorming.
- **Reconcile with the existing fact-based `ATTENDANCE_DED` rule:** today attendance flows in as aggregated facts (`AbsentDays`, `LateHours`…) consumed by a seeded rule. Moving to visible records means the payroll run must consume the **records**, not re-derive the same deduction from facts — avoid double-counting. The migration path for `ATTENDANCE_DED` is a key design decision.

### 4. Cutoff carry-over enforcement (begins here)
- Each transaction has a `Date`. At run time, transactions with `Date > period.cutoffDate` are **carried to the next period** (tagged, not dropped), honoring `PayrollDefinitionVersion.CutoffDay` / `CarryToNextPeriod` from sub-project 1.
- Surface the "transactions after cutoff will carry to next payroll" message in the run/transaction UI.
- Applies to attendance deductions, additions, deductions, loans, expenses, overtime, leave-without-pay.

### 5. Engine integration
- Run calculation reads the period's approved addition/deduction records (respecting cutoff + the calc-settings toggles stored in sub-project 1's `CalcSettingsJson`: `includeAdditions`, `additionsInGross`, `includeDeductions`, `includeAttendanceDeductions`, …).
- Feed records into the fact provider / payslip components and ultimately the append-only ledger, preserving the immutable-run model (records resolved against the frozen `PayrollRunPopulation`).

## API surface (from the master spec §13)
```
GET  /api/payroll/additions
POST /api/payroll/additions
GET  /api/payroll/deductions
POST /api/payroll/deductions
```
(plus PUT/approve/attachment endpoints, TBD in the spec)

## Acceptance (slice of the master acceptance tests)
- Attendance penalty applied → a visible `PayrollDeduction` appears in `/payroll/deductions` with source `Attendance` + reference, **before** approval.
- Additions and deductions show **separately** on the run details.
- A transaction dated after the cutoff day is carried to the next period (not silently included or dropped).
- No deduction is applied without a traceable record.

## Key design decisions to settle in brainstorming
1. **`ATTENDANCE_DED` migration** — replace the fact-based rule with record consumption, or run both with de-dup? (Avoid double-counting.)
2. **Attendance→deduction trigger** — Completion-Effects executor vs. direct attendance-service hook vs. both.
3. **Recurring transactions** — how `IsRecurring` materializes per period (template vs. auto-generated records).
4. **Cutoff carry-over storage** — re-stamp `PayrollPeriod` on carry, or compute at run time from `Date`.
5. **Approval workflow** — reuse the existing Approval Center for addition/deduction approval, or a lightweight status field.

## Out of scope (later sub-projects)
- Excluded-employees reasons, KPI cards, full run-details table, lifecycle timeline (sub-project 3).
- Payslip PDFs (sub-project 4). Exports (sub-project 5).

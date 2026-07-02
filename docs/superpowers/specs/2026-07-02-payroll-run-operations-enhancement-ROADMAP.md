# Payroll Run Operations Enhancement — Roadmap Addition

**Date:** 2026-07-02
**Status:** Roadmap / scoping note — requirements captured, NOT yet finalized specs. Each sub-project below still runs its own **brainstorm → spec → writing-plans → build** cycle.
**Parent:** `2026-06-30-payroll-engine-redesign-master.md`
**Builds on (shipped):** sub-project 1 (types/scope/cutoff), 2A (transaction records), 2C (consume/post/reverse), **2D (attendance→deduction records, deployed 2026-07-02)**.

> This note folds eight requested requirement areas into the existing payroll decomposition. Several items are **already built** by 2D — flagged inline so we enrich rather than duplicate. New work is grouped into the sub-projects it belongs to.

---

## Status legend
- ✅ **DONE** — already built/deployed (2A/2C/2D).
- 🟡 **PARTIAL** — foundation exists; this adds to it.
- 🔵 **NEW** — not yet built.

---

## Area 1 — Payslip per employee → **Sub-project 4 (Payslips)** 🔵

Every run item generates a printable payslip per employee.

**Foundation that exists:** `PayrollPayslip` already stores the immutable per-employee snapshot with the full component breakdown (`ComponentsJson` — including the per-record `TXN:` lines for additions/deductions/attendance from 2C/2D). The **document engine** (QuestPDF + JSON block templates) already renders request/leave/termination PDFs. **`CompanyProfile`** (Company Settings module) supplies logo + official details. Employees have a DB-backed file store + documents area.

**New work:**
- A **payslip document template** (block model) in the document engine — no payslip template exists yet.
- Render one payslip per run item; map the required fields from existing data:
  | Payslip field | Source |
  |---|---|
  | Company logo + official details | `CompanyProfile` |
  | Employee name/number, department, job title | run population snapshot / `Employee` |
  | Payroll period | `PayrollRun.PeriodStart/End` |
  | Basic / Allowances / Additions / Deductions / GOSI / Gross / Net | payslip `ComponentsJson` (rule components) + totals |
  | Attendance deductions | `TXN:` lines, `SourceModule=Attendance` (2D ✅) |
  | Overtime additions | `TXN:` lines, `SourceModule=Attendance`, `Kind=Addition` (**Area 5 🔵**) |
  | Loans / installments | consumed loan transactions / ledger `ReferenceType` |
  | Approval / posting date | `PayrollRun.ApprovedAt` / payslip posting metadata (2C ✅) |
- **Actions:** Preview · Print · Download PDF · **Store in employee documents** (reuse the file store + employee-documents link).
- **Permissions:** `Payroll.Payslip.View`, `Payroll.Payslip.Print`, `Payroll.Payslip.Download`.

**Open decisions:** payslip template as a first-class document-template type vs a dedicated renderer; store-on-approve automatically vs on-demand; per-run bulk generation + caching.

---

## Area 2 — Payroll amendment after approval → **Sub-project 6 (Run Void / Amend / Reissue)** 🔵 (NEW sub-project)

Approved/Completed runs are immutable; controlled change is via void/amend/reissue, not editing.

**Foundation that exists:** the run state machine (`Draft → Preview → Validated → PendingApproval → Approved → Executing → Completed/Failed/Cancelled/Locked/Archived`) and, at the **transaction** level, 2C's reversal model (`Posted → Reversed` via append-only ledger counter-entries, immutable originals). Area 2 raises this to the **run** level.

**New work:**
- **New run lifecycle capabilities + statuses:** `Voided`, `Amending`, `Reissued` (in addition to `Approved`). A **Void** reverses the run's ledger postings (reuse `IFinancialLedger.ReverseAsync`) and flips its consumed transactions `Posted → Reversed` (2C mechanics). **Amend** creates a **new run referencing the old** (`AmendsRunId`/`SupersededByRunId`), recalculates affected employees, and **Reissues** payslips.
- Old run stays **immutable**; the new run points back to it; the ledger nets via counter-entries.
- **Recalculate affected employees only** (delta) vs full re-run — a key design decision.
- **Permissions:** `Payroll.Run.Void`, `Payroll.Run.Reissue`.
- **Audit:** who voided/reissued, old-run vs new-run linkage (Area 8).

**Open decisions:** new `PayrollRunState` values vs a separate `PayrollRunAmendment` entity; how partial recalculation interacts with the frozen population + immutable payslips; whether Void requires prior un-posting or only ledger reversal; interaction with **diagnosis bug #4** (creating transactions against a closed period should route into an amendment, not orphan — see `payroll-2c-diagnosis`).

---

## Area 3 — Add additions/deductions from the run page → **Sub-project 3 (Run details)** 🟡

Quick actions on the run details page: **Add Addition · Add Deduction · Add Attendance Deduction · Add Overtime Addition**, creating `PayrollTransaction` records linked to the selected employee + payroll period.

**Foundation that exists:** `PayrollTransactionService.CreateAsync` (✅ 2A) + the source-agnostic consumer (✅ 2C) already create and consume records. "Add Attendance Deduction / Overtime Addition" reuse the master-data types (ABSENCE/LATE/SHORTAGE ✅ 2D; OVERTIME 🔵 Area 5).

**New work:**
- FE quick-action controls on `/payroll/runs/[id]` scoped to a selected employee + the run's period.
- A create path that stamps origin (Area 8) and links employee + period.
- **Permission:** `Payroll.Transaction.CreateFromRun`.
- **Run-state guard (diagnosis bug #4):** creating against an **already-approved** run's period must be blocked or routed to an **amendment** (Area 2), never silently orphaned. This finally closes bug #4.

---

## Area 4 — Attendance daily penalty actions → **Sub-project 2E (extends 2D)** 🟡

On the daily attendance page: **Calculate absence deduction · Calculate late deduction · Calculate shortage deduction · Calculate overtime addition**, with an optional target payroll-month selector. Results are **visible `PayrollTransaction` records** (deductions → Deductions page; overtime → Additions page). No hidden effects.

**Foundation that exists (✅ 2D):** `AttendanceWageCalculator` (computes per-day absence/late/shortage **and OvertimeMinutes**), `AttendanceDeductionSyncService` (idempotent per-employee/period/kind materialization, born Approved, `SourceModule=Attendance`, `ReferenceType=AttendancePeriodPenalty` + per-day drill-down reference rows), the Sync-Now endpoint, and the `Payroll.Configure`-gated sync. 2D materializes at the **period** level.

**New work:**
- **Daily, per-action** buttons on the attendance page (vs 2D's period sync), with target-month selection → call a per-day/per-kind sync path (reuse the calculator + sync service; scope to a day or an employee/day set).
- **Overtime → Addition** (🔵 Area 5) — 2D only produced deductions; add the addition path.
- **Permission:** `Attendance.PayrollImpact.Create`.

**Open decisions:** per-day records (`ReferenceType=AttendanceRecord`) vs 2D's per-period aggregate (drill-down already stores per-day rows) — reconcile so a daily action doesn't create a competing record shape; how the target-month selector interacts with cutoff resolution.

---

## Area 5 — Attendance/Payroll integration (the record contract) → **Sub-project 2E (extends 2D)** 🟡

Every attendance-driven payroll impact is a `PayrollTransaction`:
- Absence → Deduction ✅ (2D)
- Late arrival → Deduction ✅ (2D)
- Shortage hours → Deduction ✅ (2D)
- **Overtime → Addition** 🔵 (new — mirror the deduction sync with `Kind=Addition`, master-data `AdditionType` code `OVERTIME`)
- **Approved excuse → cancels/zeroes the related deduction before posting** 🔵 (new trigger)

Each transaction includes: `SourceModule=Attendance`; `ReferenceType` = `AttendanceRecord` or `AttendancePeriodPenalty`; `ReferenceId`; payroll period; breakdown details. **✅ All of these ship in 2D** (via `PayrollTransaction` provenance columns + the `PayrollTransactionAttendanceReference` snapshot table) for the three deduction kinds.

**New work:**
- **Overtime addition** path end-to-end (calculator already yields `OvertimeHours`; needs an `AdditionType` `OVERTIME` seed + sync path + Additions-page surfacing).
- **Approved-excuse trigger:** when an excuse/leave is approved for a day, attendance recalcs to a non-penalty state; a **re-sync before posting** cancels/zeroes the stale record (2D's **cancel-on-zero** already does this once the attendance no longer shows the penalty — the new work is the *trigger* that fires the re-sync on excuse approval, e.g. a Completion Effect or an attendance-service hook, and the guard that it only applies **before** the period is Posted).

---

## Area 6 — Payroll exports → **Sub-project 5 (Exports)** 🔵

Run exports in **Excel · PDF** now (**CSV/TXT later**), for report types: **Payroll summary · Employee detailed payroll · Payslips · Additions report · Deductions report · Attendance impact report · Excluded employees report**.

**Foundation that exists:** master-spec principle #4 already envisions an **exporter handler registry** (DI-discovered handlers, like the completion-effects pattern). **ClosedXML** is already used (employee Excel export). The document engine covers PDF. `PayrollRunPopulation` carries exclusion reasons (excluded-employees report). Attendance-impact data = the `SourceModule=Attendance` transactions + reference table (2D).

**New work:**
- `PayrollExportJob` + exporter registry (Excel/PDF handlers per report type).
- Field set: employee data, basic, allowances, additions, deductions, gross, net, payment method, **Bank/IBAN gated by permission**.
- **Permission:** `Payroll.Export` (and IBAN/bank behind a finer permission).

**Open decisions:** synchronous vs background (Hangfire) generation; field-picker UX; which reports are Excel-only vs PDF-only.

---

## Area 7 — Permissions (cross-cutting) 🔵

Register in the existing Access Management permission system (deny-wins resolver, JWT claims). New permissions:
- `Payroll.Payslip.View`, `Payroll.Payslip.Print`, `Payroll.Payslip.Download` (Area 1)
- `Payroll.Run.Void`, `Payroll.Run.Reissue` (Area 2)
- `Payroll.Export` (Area 6)
- `Payroll.Transaction.CreateFromRun` (Area 3)
- `Attendance.PayrollImpact.Create` (Area 4)

Each is seeded via the permission seeder and granted to the appropriate default roles; UI gates via the existing `usePermission`/`AccessGuard`.

---

## Area 8 — Audit (cross-cutting) 🔵/🟡

Every action logged via `IAuditLogService`. Required trails:
- **Who created an addition/deduction, and from which screen** — needs a new **origin/source-screen field** on the transaction create args (e.g. `Origin` ∈ {RunPage, AttendanceDaily, DeductionsPage, AdditionsPage, Import}); today `SourceModule` records the *system* origin, not the *screen*. 🔵
- **Which attendance record caused it** — `ReferenceId` + reference table ✅ (2D).
- **Which payroll consumed it** — `PayrollRunId`/`PostedAt`/`PostedBy` on the transaction ✅ (2C).
- **Who voided/reissued a payroll, and old-vs-new linkage** — `AmendsRunId`/`SupersededByRunId` + audit entries 🔵 (Area 2).

---

## Revised decomposition (build order)

| # | Sub-project | Status | Covers |
|---|---|---|---|
| 1 | Types + Scope + Cutoff | ✅ shipped | — |
| 2A/2C/2D | Records + consume/post/reverse + attendance→deduction | ✅ shipped | Areas 4/5 (deductions), most of 8 |
| **2E** | **Attendance daily actions + Overtime addition + excuse trigger** | 🔵 next | Areas 4, 5 (overtime + excuse) |
| **3** | **Run details + run-page quick actions** (+ close bug #4) | 🔵 | Area 3, exclusions/KPIs/timeline |
| **4** | **Payslips** | 🔵 | Area 1 |
| **5** | **Exports** | 🔵 | Area 6 |
| **6** | **Run Void / Amend / Reissue** | 🔵 | Area 2 |
| — | Permissions + Audit-origin | 🔵 cross-cutting | Areas 7, 8 — folded into each sub-project above |

Suggested order: **2E → 3 → 4 → 5 → 6** (records/attendance completeness first, then the run-details surface the quick actions live in, then payslips, then exports, then the heaviest lifecycle change last). Permissions/audit-origin land inside whichever sub-project introduces the action.

## Cross-cutting principles (unchanged, from the master spec)
Immutability & reproducibility · no duplicate fields (new catalogs = new `MasterDataObjectType`) · strongly-typed hot columns + JSON for flexible settings · decoupling via providers/registries · **traceability — no hidden payroll effects** (Areas 4/5 are the enforcement) · compatibility with the run state machine / ledger / snapshot contracts.

# Payroll Engine Redesign — Master Vision

**Date:** 2026-06-30
**Status:** Vision / decomposition (each sub-project has its own spec → plan → build cycle)

## Goal

Build a complete operational payroll layer on top of the existing Financial Engine
(Passes 1–4): configurable payroll types, rich employee selection, visible/traceable
additions & deductions, attendance-deduction sync, run lifecycle with exclusions,
payslip PDFs, and exports. No mock data, no placeholders.

## Architectural foundation (already built — preserve, do not rewrite)

The Financial Engine is mature and is the substrate everything builds on:

- **`PayrollDefinition` + `PayrollDefinitionVersion`** — versioned, immutable config
  snapshot a run pins to. This *is* the "Payroll Type" (enriched, not replaced).
- **`PayrollRun` + run state machine** — Draft → Preview → Validated → PendingApproval
  → Approved → Executing → Completed / Failed / Cancelled (+ Locked/Archived). The
  spec's "Calculated" status = the engine's `Preview`.
- **`PayrollPayslip`** — immutable per-employee snapshot with full component breakdown.
- **Append-only `FinancialLedgerEntry`** with reversals; AST rule engine
  (`RuleSet`/`RuleSetVersion`/`Rule`, stored as source text + compiled AST JSON);
  dependency-ordered evaluation; validators; batch posting (resumable/idempotent);
  audit; MediatR events.
- **`PayrollFactProvider`** — builds per-employee facts (basic, allowances, additions,
  deductions, GOSI, attendance aggregates). Attendance integrates here today as facts
  consumed by a seeded `ATTENDANCE_DED` rule.
- **Document engine** (QuestPDF + JSON block templates) — renders request/leave/
  termination PDFs. No payslip template yet.
- **Master-data engine** — one generic `MasterDataItem` table keyed by `ObjectType`
  (30 types incl. Grade, CostCenter, Tag, ShiftType, Nationality, PaymentMethod,
  PayrollGroup). The canonical way to make something "configurable by the customer."
- **Completion Effects Engine** — plug-in side-effect orchestrator for *request
  approvals* (`IEffectExecutor` discovered via `AddEffectExecutorsFromAssembly`).
  Payroll runs are a separate subsystem; payroll integrates by reading artifacts
  effects create (e.g. loan installments) and, later, by registering its own executors.

## Cross-cutting principles (apply to every sub-project)

1. **Immutability & reproducibility** — config changes publish a new
   `PayrollDefinitionVersion`; runs pin the version and the `RuleSetVersion`. Runs
   snapshot their resolved employee population so future org changes never alter
   historical payrolls.
2. **No duplicate fields** — reuse canonical fields (master-data, existing version
   columns). New configurable catalogs are new `MasterDataObjectType`s, not new tables.
3. **Strongly-typed columns for hot/queryable fields, JSON for flexible/advanced
   settings.**
4. **Decoupling via providers/registries** — payroll depends on abstractions
   (`IScopeEngine`, exporter handler registry), never directly on other modules'
   schemas. Owning modules supply implementations, discovered via DI assembly scans
   (same pattern as completion effects).
5. **Traceability** — every deduction is a visible, sourced record before approval; no
   hidden computed deductions (philosophy change from fact-only to record-backed —
   sub-project 2).
6. **Compatibility** — stays compatible with the Completion Effects Engine and the
   existing run state machine / ledger / snapshot contracts.

## Decomposition (build in this order; each independently shippable)

1. **Payroll Types + Selection Scope + Cutoff** *(spec:
   `2026-06-30-payroll-types-scope-cutoff-design.md`)* — Settings foundation. Enrich
   `PayrollDefinition`/version with category, calc settings, cutoff, rich selection
   scope; pluggable Scope Engine; config versioning (clone/publish/simulate);
   run-population snapshot; Settings UI. **Builds the ground everything else stands on.**
2. **Additions & Deductions module + Attendance→Deduction sync** — record-based,
   traceable `PayrollAddition`/`PayrollDeduction` entities; `/payroll/additions` &
   `/payroll/deductions` pages; attendance penalties create visible deduction records
   with source/reference; cutoff carry-over enforcement begins here.
3. **Run engine wiring + run details** — consume types/scope/cutoff/records; compute
   exclusions with reasons (enrich `PayrollRunPopulation`); KPI cards, full employee
   table, excluded-employees section; lifecycle timeline UI.
4. **Payslips** — payslip document template (block model), render per item, store in
   employee documents.
5. **Exports** — PDF/Excel/CSV/TXT, report types (summary, payslips, bank transfer,
   Mudad, cash sheet, deductions/additions/excluded reports), field picker,
   permission-respecting; `PayrollExportJob`.

Each sub-project: brainstorm → spec → writing-plans → implement → verify → ship.

### Progress + roadmap expansion (2026-07-02)

Shipped: **1** (types/scope/cutoff), **2A** (transaction records), **2C** (consume/post/
reverse), **2D** (attendance→deduction records — deployed). The "Payroll Run Operations
Enhancement" requirements (payslips, run void/amend/reissue, run-page quick actions,
daily attendance actions + overtime + excuse, exports, new permissions, audit-origin)
are captured and mapped to sub-projects in
`2026-07-02-payroll-run-operations-enhancement-ROADMAP.md`. Expanded decomposition:

- **2E** — Attendance daily payroll actions + Overtime→Addition + approved-excuse
  trigger (extends 2D).
- **3** — Run engine wiring + run details + run-page quick actions (Add Addition/
  Deduction/Attendance-Deduction/Overtime); closes diagnosis bug #4 (run-state guard).
- **4** — Payslips (per-item PDF: preview/print/download/store-in-documents).
- **5** — Exports (Excel/PDF now; CSV/TXT later; summary/detailed/payslips/additions/
  deductions/attendance-impact/excluded reports; IBAN behind permission).
- **6** — Run Void / Amend / Reissue (post-approval controlled change; new statuses
  Voided/Amending/Reissued; old run immutable, new run references it).

Suggested order: 2E → 3 → 4 → 5 → 6. New permissions + audit-origin fold into whichever
sub-project introduces the action.

## Acceptance (whole programme — from the original spec)

Admin creates a Payroll Type (Monthly), sets cutoff = 27, creates a run for a
department; system includes matching employees and excludes invalid ones with reasons;
attendance deductions sync as visible records; additions/deductions show separately;
gross/net compute correctly; payslip PDF prints; exports (Excel/PDF/TXT) succeed;
approved payroll stores payslips in employee documents.

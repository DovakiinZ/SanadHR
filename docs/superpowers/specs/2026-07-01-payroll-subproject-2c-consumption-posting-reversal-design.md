# Sub-project 2C — Transaction Consumption, Posting & Reversal (Design)

**Status:** Approved design — ready for implementation planning.
**Date:** 2026-07-01
**Parent:** `2026-06-30-payroll-engine-redesign-master.md`
**Overview:** `2026-06-30-payroll-subproject-2-additions-deductions-OVERVIEW.md`
**Predecessor:** `2026-06-30-payroll-subproject-2a-transaction-records-design.md` (2A — records + lifecycle, shipped & deployed)
**Branch:** `feat/financial-engine`

> Built via the standard **brainstorm → spec → writing-plans → subagent build** cycle (same as 2A).

---

## 1. Goal

Make the addition/deduction **records** created in 2A actually flow through a payroll run: **consumed** by the run as visible per-record lines, **posted** to the immutable ledger on execution, and **correctable after the fact via reversal** rather than by editing a closed period. This delivers the user's reported issue #4 — "the payroll needs to be edited even when it's approved" — through an auditable reversal/correction model instead of reopening posted runs.

This is the genuine, never-built "engine consumption" half of sub-project 2 (the original "2B"). 2A shipped the records and a lifecycle whose `Posted`/`CarriedForward`/`Reversed` states and `PayrollRunId`/`PostedAt`/`PostedBy`/`LedgerEntryId`/`ReversesTransactionId`/`ReversalReason` columns were left **inert pending this sub-project**. 2C makes them live.

## 2. Background — the current gap

- Runs compute additions/deductions as **aggregate facts** (`TotalAdditions`/`TotalDeductions`) derived from the employee's stored fields in `PayrollFactProvider`. The `/payroll/additions` and `/payroll/deductions` records are **not connected to any run**.
- The run freezes per-employee payslips at **Calculate** (`PayrollRunEngine`), posts components to the ledger at **Execute** (`PayrollExecutionEngine` → `PayslipLedgerMapper` → `PayrollItemExecutor`), and is **immutable once Approved** (`PayrollRunStateMachine`, no `Approved → Draft` path).
- The transaction lifecycle (`PayrollTransactionStateMachine`) already defines `Approved → Posted → Reversed`; consumption/posting/reversal simply drive those transitions.

## 3. Scope (the "spine")

In scope:
1. **Consume at Calculate** — inject each employee's eligible `Approved` transactions into the frozen payslip as individual components.
2. **Post at Execute** — one ledger entry per consumed transaction; flip `Approved → Posted` with full posting metadata.
3. **Reverse on demand** — reverse a `Posted` transaction (counter ledger entry + `Posted → Reversed`), optionally create a correction in the next open period.
4. **Cutoff carry-over + impact preview** — resolve a transaction's target period from `EffectiveDate` + the definition's cutoff at run time, and surface it in the UI.

Explicitly **out of scope** (future sub-projects 2D+):
- Batch Excel import (overview §16)
- Duplicate / conflict detection (overview §9)
- `IPayrollTransaction` abstraction + transaction priority (overview §11/§12)
- Attachment metadata enrichment (overview §15)
- **Attendance → deduction record migration** — attendance penalties keep flowing through the existing fact-based `ATTENDANCE_DED` rule. The per-record path in 2C is **manual additions/deductions only**, which avoids the double-count reconciliation entirely. Migrating attendance to records is its own later slice.

## 4. Architecture

Three touch-points on the existing run lifecycle, reusing the append-only `FinancialLedgerEntry` ledger and the `PayrollTransactionStateMachine` unchanged.

```
Create ─ Calculate ─── Validate ─ Approve ─ Execute ─ Completed/Locked
            │                                  │
   [CONSUME approved txns]            [POST txns: ledger entry
   inject per-record components        + Approved→Posted + metadata]
            │
   [cutoff carry-over resolved here]

   (any time, on a Posted txn)
   REVERSE → counter ledger entry + Posted→Reversed
           → optional correction txn (Draft) in next open period
```

### 4.1 Consumption — `PayrollTransactionConsumer` (new, `HR.Infrastructure/Engines/Finance`)

`Task<IReadOnlyList<ConsumableTransaction>> GetConsumableAsync(PayrollPeriod period, IReadOnlyCollection<Guid> employeeIds, PayrollDefinitionVersion version, CancellationToken ct)`

- Selects `PayrollTransaction` where `Status == Approved`, `EmployeeId` in the run population, and the **resolved target period == the run period**.
- **Resolved target period** is computed at run time from `EffectiveDate` + the definition's `CutoffDay` / `CarryToNextPeriod` (`PayrollDefinition.cs:50,54`), **not** the create-time `TargetPeriodYear/Month` stamp. Rule: if `CarryToNextPeriod` and `EffectiveDate.Day > CutoffDay`, the transaction belongs to the **next** period. Re-deriving at run time means changing the cutoff config never strands a record.
- Returns lightweight rows (transaction id, kind, type label, amount, effective date) for the calculate step. Read-only; no writes.

Called from the snapshot/calculate path in `PayrollRunEngine.CalculateAsync` (or the fact/snapshot builder it delegates to), so the consumed set is frozen into the payslip alongside the rule-computed components.

### 4.2 Payslip component mapping

Each consumed transaction becomes one payslip `PayComponent` line:
- `Kind = Earning` for additions, `Deduction` for deductions (`PayComponentKind`, `FinanceEnums.cs:43`).
- Labelled by its master-data `AdditionType`/`DeductionType` (so run details show them **separately**, by type).
- Carries `SourceTransactionId` so posting and reversal can trace back to the record.
- **Gross treatment:** additions respect the existing sub-project-1 `CalcSettingsJson` toggles (`includeAdditions`, `additionsInGross`); deductions are applied post-gross. Per-type GOSI/tax treatment is **deferred** — additions are uniformly treated per the toggle, not per type.
- These per-record components are **additive to** the rule-computed BASIC/ALLOWANCES/GOSI. The legacy aggregate `TotalAdditions`/`TotalDeductions` facts that derive from employee stored fields are **superseded for manual records**: to avoid double counting, the seeded `ADDITIONS`/`DEDUCTIONS` rule components must no longer also sum manual records. (Migration of the standard rule set / fact provider is part of implementation — see §8 open item.)

### 4.3 Posting — extend `PayslipLedgerMapper` + `PayrollItemExecutor`

- `PayslipLedgerMapper` emits **one posting per consumed transaction** (Earning = Credit, Deduction = Debit), with a deterministic `EntryNumber` incorporating the transaction id and the transaction id stored on/linked to the entry.
- On execute, `PayrollItemExecutor` (already scoped + idempotent per payslip) additionally, for each consumed transaction: writes the ledger entry, sets the transaction `Approved → Posted`, and stamps `PayrollRunId`, `PostedAt`, `PostedBy`, `LedgerEntryId` (`PayrollTransaction.cs:58–61`). Idempotent: skip a transaction already `Posted` / already referenced by a ledger entry, so re-running a failed item never double-posts.

### 4.4 Reversal — `IPayrollTransactionReversalService` / `PayrollTransactionReversalService` (new)

`Task<ReversalResult> ReverseAsync(Guid transactionId, string reason, bool createCorrection, decimal? correctedAmount, CancellationToken ct)`

- Validates the transaction is `Posted` (else `DomainException`).
- Writes a **counter ledger entry** (opposite sign, `IFinancialLedger.Reverse` / counter-post referencing the original entry).
- Creates a **counter `PayrollTransaction`** with `ReversesTransactionId = original.Id`, opposite effect, `ReversalReason = reason`; transitions the original `Posted → Reversed`.
- If `createCorrection`, creates a new `Draft` transaction for `correctedAmount` targeting the **next open period** (resolved from the current date + cutoff). If there is no open period to receive it, throw `DomainException`.
- Gated by `Payroll.Approve` (a financial correction).

### 4.5 Frontend

- **Run details** (`/payroll/runs/[id]`): additions/deductions render as **separate per-record lines** (type, employee, amount, source transaction link), not a single lumped total.
- **Reverse action**: on a `Posted` transaction, a Reverse button (gated by `Payroll.Approve`) opens a reason dialog with an optional "create correction of amount X in <next period>" path.
- **Impact preview**: the transaction create/edit form shows the resolved target period live — e.g. *"Will affect: August 2026 payroll — created after the July cutoff (day 27)."*

## 5. State & lifecycle

`PayrollTransactionStateMachine` is used **unchanged**:
- Consumption reads only `Approved`.
- Posting: `Approved → Posted`.
- Reversal: `Posted → Reversed`; the counter-transaction is born `Posted` (or `Approved → Posted` in the same operation) so it nets immediately.
- Runs stay immutable once `Approved` — **no reopen path** is added (consistent with the chosen reversal model).

## 6. Error handling

All new business-rule failures throw **`DomainException`** (the 422 type added in the 2C Track-1 hotfix, `0f7cd35`), so the real reason reaches the client. Examples: "Transaction is not Posted; only posted transactions can be reversed.", "No open payroll period is available to receive the correction.", "Transaction is already posted by run <id>." Illegal lifecycle moves continue to surface via the state-machine exceptions (→ 409). Consumption/posting run inside the existing execute scope and add no new permission; reversal requires `Payroll.Approve`.

## 7. Testing

xUnit in `backend/tests/HR.Domain.Finance.Tests` (Npgsql-sensitive paths noted — InMemory will not catch timestamptz issues):
1. **Consumption filter** — only `Approved` + in-population + resolved-in-period transactions are returned; cutoff carry-over moves an after-cutoff transaction to the next period.
2. **Per-record components** — two approved additions of different types appear as two separate `Earning` lines on the payslip, each with its `SourceTransactionId`.
3. **Posting** — Execute writes exactly one ledger entry per consumed transaction, sets `Posted` + metadata; a re-run (simulated failed item) does **not** double-post (idempotent).
4. **Reversal** — reversing a posted transaction nets the employee's ledger balance to zero for that pair, transitions the original to `Reversed`, and (when requested) creates the correction `Draft` in the next open period.
5. **Reversal guards** — reversing a non-`Posted` transaction throws `DomainException`; reversal without an open period for a requested correction throws `DomainException`.

## 8. Migration & open implementation items

- **No new migration expected** — all required columns (`PayrollRunId`, `PostedAt`, `PostedBy`, `LedgerEntryId`, `ReversesTransactionId`, `ReversalReason`) shipped in 2A's `PayrollTransactions` migration. To be **confirmed during planning**; if the ledger entry needs a `SourceTransactionId` column for the per-record link, that is the only candidate migration.
- **Double-count avoidance** — the seeded `STD_MONTHLY` `ADDITIONS`/`DEDUCTIONS` rule components and `PayrollFactProvider.TotalAdditions/TotalDeductions` must be reconciled so manual records aren't counted both as facts and as injected components. Decide during planning: either (a) drop manual additions/deductions from the fact aggregation (keep only employee-intrinsic allowances), or (b) zero the seeded aggregate rule for manual kinds. Attendance (`ATTENDANCE_DED`) is untouched.
- **Endpoint** — add `POST /api/payroll/transactions/{id}/reverse` on `PayrollController` (gated `Payroll.Approve`), plus run-details DTO enrichment to expose per-transaction lines.

## 9. Acceptance criteria

- An approved addition appears as its own line on the run's payslip and on the ledger after execution, traceable to its transaction id.
- A transaction dated after the cutoff day is consumed by the **next** period's run, and the UI says so before approval.
- A posted transaction can be reversed: the ledger nets to zero for that pair, the original shows `Reversed`, and an optional correction appears as a `Draft` in the next open period.
- Approved/executed runs remain immutable; no posted transaction is edited in place.
- No manual addition/deduction is counted twice (fact + record).

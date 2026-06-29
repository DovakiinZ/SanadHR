# Sub-project 1 — Verification Notes (Payroll Types + Selection Scope + Cutoff)

Plan: `2026-06-30-payroll-types-scope-cutoff.md`. Branch: `feat/financial-engine`.

## Automated verification (done in this session)

- **Full solution build:** `dotnet build backend/HR.sln` → 0 errors (10 pre-existing
  warnings: AutoMapper CVE + a stray async-without-await in AuthController, both
  unrelated to this work).
- **Full test suite:** `dotnet test backend/HR.sln` → **151/151 passing**, pristine:
  - `HR.Domain.Finance.Tests`: 109 (incl. new: SelectionScopeJson ×6, ScopeEngine ×7,
    PayrollTypeService ×5, DayBasisProration ×3)
  - `HR.Modules.Employees.Tests`: 18 (incl. EmployeeScopeProviders ×3)
  - `HR.Modules.Workflows.Tests`: 24
- **Frontend build:** `npm run build` → exit 0, 64/64 pages (run during Tasks 12–13);
  `/settings/payroll/types` and `/settings/payroll/types/[id]` routes registered.

## Manual / environment-dependent verification (REQUIRES a running app + DB — not run autonomously)

These need a live API + database (and, for the UI, a browser). They were deliberately
NOT executed against the production Azure DB during the autonomous build.

### A. Apply the migration (DEFERRED — needs the DB secret)
```
dotnet ef database update -p backend/src/HR.Infrastructure -s backend/src/HR.Api \
  --connection "$env:ConnectionStrings__DefaultConnection"
```
Applies `PayrollTypesAndScope` (new columns + `engine_payroll_run_population` + FK +
backfill of `EmployeeFilterJson → SelectionScopeJson`). Run against local first, then
Azure. **Re-bootstrap / re-seed master-data** afterward so the `PayrollTypeCategory`
and `PayrollExportFormat` catalogs exist BEFORE `StandardPayrollSeeder` runs (otherwise
the MONTHLY type's category/export-format FKs seed as null — see Task 10 note).

### B. Runtime DI smoke (closes the gate the reviewers flagged for Tasks 11/13)
With the API running, confirm no `InvalidOperationException` at startup and:
- `GET /api/payroll/scope/dimensions` (Payroll.View) returns the 8 available dimensions
  (Department, Branch, JobTitle, EmploymentType, ContractType, PaymentMethod,
  Nationality, Status) PLUS the 7 disabled ones (Tag, CostCenter, Grade, Shift,
  Project, BusinessUnit, Company) each with `isAvailable:false` + a note.
- `IPayrollTypeService` / `IScopeEngine` resolve (any `/types` call exercises them).

### C. Backend endpoint smoke (acceptance steps 1–6)
With a `Payroll.Configure` token:
1. `POST /api/payroll/types` `{code:"OPS",name:"Ops Monthly",nameAr:"شهري العمليات",categoryId:<REGULAR>}` → 201 + id.
2. `PUT /types/{id}/versions/{vid}` with a Department-include scope, `cutoffDay:27`, `dayBasis:"CalendarMonth"`.
3. `POST /scope/resolve` with that scope → `includedCount>0` matching the department.
4. `POST /types/{id}/versions/{vid}/publish` → 200; `GET /types/{id}` shows version Published + `currentVersionId` set.
5. `POST /api/payroll/runs` `{definitionId:{id},year,month}` → run created; `engine_payroll_run_population` has frozen rows; `run.employeeCount` == resolved population.

### D. Immutability check (acceptance)
After creating the run, change an employee's department in the DB → re-`GET` the run →
`employeeCount` and the frozen population are unchanged.

### E. Frontend smoke (browser)
- `/settings/payroll/types` lists the type; "New type" gated by `Payroll.Configure`.
- Detail page: scope builder shows available + 7 disabled (greyed) dimensions; live
  count updates as criteria change; Clone/Publish/Simulate work; Published versions are
  read-only ("Clone to edit").
- **Excluded-allowances round-trip:** select excluded allowance types, save, reload →
  selections persist (the Task 13 fix; confirm `calcSettingsJson` carries
  `excludedAllowanceTypeIds`).
- **Status dimension resolves:** select "نشط" (Active), save, simulate/resolve → count
  is non-zero (confirms the `00000000-…-01` GUID contract end-to-end).
- Confirm the test admin JWT carries `Payroll.View` and `Payroll.Configure` (newly
  seeded permission — re-provision permissions if needed).

## Open Minor items rolled up for the final whole-branch review
See `.superpowers/sdd/progress.md` (MINOR/CARRY-FORWARD lines):
- Pre-existing `AssetType` double-seed in `MasterDataDefaults.cs`.
- `DailyWageFor` uses `Math.Round` ToEven vs the AwayFromZero constraint (intermediate
  4dp rate; matches pre-existing dailyWage/hourlyWage style) — adjudicate.
- No behavioral test for `StatusScopeProvider` GUID round-trip (frontend contract).
- `WorkingCalendarId` not editable via `UpdateDraftVersionAsync`.
- Cosmetic: unused `using` in `PayrollTypeDtos.cs`; `UpdateHeaderRequest.Status` default
  "Active"; `ResolveScopeRequest.ScopeJson` no `[Required]`.

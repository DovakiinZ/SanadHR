# Workflow Builder & Execution Engine

A decoupled HR approval-workflow system: a **visual timeline builder** (Next.js) that designs
approval chains, and a **CQRS execution engine** (.NET 8 + MediatR) that runs and tracks them
asynchronously. It is a self-contained module that lives alongside — and deliberately does not touch
— the pre-existing graph/Request-Center workflow engine.

> The repo already ships a richer node/edge workflow engine (`HR.Domain.Engines.Workflows`). This
> module is a separate, spec-shaped **linked-list** engine (`RootStepId` + `NextStepIdSuccess` /
> `NextStepIdFailure`) under its own namespace (`HR.Domain.Engines.FlowBuilder`) and table prefix
> (`flow_*`) so the two coexist with zero collisions.

---

## 1. Data model (`HR.Domain.Engines.FlowBuilder`)

| Entity | Table | Purpose |
| --- | --- | --- |
| `WorkflowDefinition` | `flow_workflow_definitions` | A versioned chain. `Code`, `Name`, `Version`, `IsActive`, `RootStepId`. |
| `WorkflowStep` | `flow_workflow_steps` | A node: `Type` (Approval/Action/Condition/End), `Config` (jsonb), `NextStepIdSuccess`, `NextStepIdFailure`, `SortOrder`. |
| `WorkflowRequest` | `flow_workflow_requests` | A running instance: `RequesterId`, `CurrentStepId`, `Status`, `Payload` (jsonb). |
| `WorkflowAuditTrail` | `flow_workflow_audit_trail` | One row per transition: `Action`, `Result`, `ToStepId`, `ActorId`, `Comment`, `OccurredAt`. |

All four inherit `TenantEntity`, so they get **multi-tenant isolation** (global query filter),
**soft delete** and **created/updated auditing** for free, exactly like the rest of the platform.

### Design decisions

- **Soft pointers, not FKs, for transitions.** `NextStepIdSuccess` / `NextStepIdFailure` are plain
  `Guid?` columns, not relationships. This lets the builder rewire the graph freely (and lets a
  step's id be client-generated and reused on save) without EF cascade-cycle constraints.
- **`Config` / `Payload` as `jsonb`.** Step configuration and request data are schema-less by nature;
  Postgres `jsonb` keeps them queryable without over-modelling.
- **Distinct table prefix + DbSet names** (`FlowDefinitions`, `FlowSteps`, …) referenced
  fully-qualified in `ApplicationDbContext` — the same pattern the Request Center already uses.

---

## 2. Backend architecture (CQRS / MediatR)

```
Controller ─▶ MediatR Command/Query ─▶ Handler ─▶ IWorkflowRunner ─▶ IWorkflowStepHandler(s)
                                              │                         (Approval / Email / Condition)
                                              └─▶ IWorkflowGraphValidator
```

### Step handlers — Strategy + Open/Closed
`IWorkflowStepHandler` has one implementation per step type (`ApprovalStepHandler`,
`EmailActionHandler`, `ConditionStepHandler`). They are registered as a DI collection and resolved
by `StepType`. **Adding a new step type is a new handler + one DI line; the engine never changes.**

The `EmailActionHandler` depends on `IWorkflowEmailSender` (default: `QueueWorkflowEmailSender`,
which enqueues onto the existing `EmailNotificationQueue`) — so e-mail delivery is swappable and
unit-testable.

### The state machine — `WorkflowRunner`
`ExecuteWorkflowStepCommand` is the spec's core command. Its handler delegates to `IWorkflowRunner`,
an explicit state machine that:

1. Applies an optional external **decision** (approve/reject) to the current blocking step.
2. **Auto-advances** through non-blocking steps (Condition/Action), following the success/failure
   pointer each step returns, until it either parks on the next blocking Approval or reaches a
   terminal (`End` step or a null pointer).
3. Writes a `WorkflowAuditTrail` row for **every** transition.

- **Atomic** — the command handler wraps the runner + a single `SaveChanges` in a DB transaction
  (via the provider's execution strategy; skipped on non-relational providers used by tests).
- **Idempotent** — a finished request never moves again; `ExecuteWorkflowStepCommand` only acts when
  the request is genuinely `InProgress` on an `Approval` step, otherwise it returns `409 Conflict`
  rather than double-applying.

### Graph validation — `WorkflowGraphValidator`
Server-side mirror of the builder's live validation: dangling pointers, self-references, a valid
root, **circular-reference detection** (white/gray/black DFS) and reachability/end-state warnings.
`UpdateWorkflowDefinitionCommand` runs it before persisting, so an invalid (e.g. cyclic) graph can
never be saved regardless of the client.

### Commands & queries
`CreateWorkflowDefinitionCommand`, `UpdateWorkflowDefinitionCommand` (saves the whole step graph),
`DeleteWorkflowDefinitionCommand`, `StartWorkflowRequestCommand`, `ExecuteWorkflowStepCommand`,
`CancelWorkflowRequestCommand`; queries for definitions, requests (paged), a single request (with
its audit trail) and the pending-approvals queue.

---

## 3. HTTP API

| Method | Route | Permission |
| --- | --- | --- |
| GET/POST | `/api/workflow-definitions` | `Workflows.View` / `Workflows.Create` |
| GET/PUT/DELETE | `/api/workflow-definitions/{id}` | `Workflows.View` / `Workflows.Edit` / `Workflows.Delete` |
| GET | `/api/workflow-requests` · `/pending` · `/{id}` | `Workflows.View` |
| POST | `/api/workflow-requests` (start) | `Workflows.View` |
| POST | `/api/workflow-requests/{id}/execute` · `/cancel` | `Workflows.Edit` |

Permissions reuse the existing seeded `Workflows.*` set (the tenant admin already holds them), so no
new permission rows / re-grants are required.

---

## 4. Frontend builder (Next.js 16 / React 19)

`src/app/(dashboard)/workflows` + `src/components/workflows/**`.

- **Timeline / Flow Builder** with `@dnd-kit/core` + `@dnd-kit/sortable` — drag to reorder steps in a
  vertical timeline; add steps from a palette; each node shows its type and success/failure targets.
- **Config side-drawer** (`Sheet`) per node for editing the step name, type-specific `Config`
  (approver role, e-mail fields, condition field/operator/value) and branch wiring.
- **`useOptimistic`** gives instant reorder feedback while React commits the real state in a
  transition (no flicker between drop and re-render).
- **Real-time validation** — `validateGraph()` (the client twin of the server validator) runs on
  every edit, surfaces circular-reference / missing-end errors, and disables Save until clean.
- **Requests view** — list + filter, a detail drawer rendering the **audit-trail timeline**, and
  approve / reject / cancel actions.

---

## 5. Tests

`backend/tests/HR.Modules.Workflows.Tests` (xUnit + EF InMemory + FluentAssertions):

- `WorkflowGraphValidatorTests` — valid/empty graphs, direct cycle, self-reference, dangling pointer,
  missing root, unreachable-step warning.
- `WorkflowExecutionTests` — start parks on the first approval; approve→complete; reject with no
  failure branch→`Rejected`; condition routes through an e-mail action; condition-false skip;
  multi-level approval parks between approvers; **executing a finished request is rejected
  idempotently**.

```bash
dotnet test backend/tests/HR.Modules.Workflows.Tests
```

> **Bug the tests caught (and which would have hit production):** the runner originally appended
> audit rows only to the tracked request's navigation collection. Because `BaseEntity` pre-sets
> `Id = Guid.NewGuid()` and EF treats Guid PKs as store-generated, EF mis-inferred those
> navigation-added rows as *existing/Modified* (not *Added*) when the parent was already tracked
> (the `Execute` path), throwing `DbUpdateConcurrencyException`. Fix: add audit rows through the
> `DbSet` explicitly, which forces `Added` state.

---

## 6. Migration

`20260619115750_FlowBuilderEngine` creates the four `flow_*` tables (additive only).

```bash
dotnet ef database update --project src/HR.Infrastructure --startup-project src/HR.Api
```

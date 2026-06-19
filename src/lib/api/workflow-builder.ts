import { apiFetch } from "../api-client";

// ── Workflow Builder engine (api/workflow-definitions + api/workflow-requests) ──
// Decoupled linked-list approval workflows: a definition is a graph of steps entered through
// rootStepId; each step points at the next step for the success / failure branch. A request is a
// running instance the backend engine advances; every transition is recorded in its audit trail.

// Mirrors HR.Domain.Engines.FlowBuilder.WorkflowStepType (serialized as integers).
export enum WorkflowStepType {
  Approval = 1,
  Action = 2,
  Condition = 3,
  End = 4,
}

// Mirrors HR.Domain.Engines.FlowBuilder.WorkflowRequestStatus.
export enum WorkflowRequestStatus {
  Pending = 1,
  InProgress = 2,
  Completed = 3,
  Cancelled = 4,
  Rejected = 5,
}

export interface WorkflowStepDto {
  id: string;
  type: WorkflowStepType;
  name: string;
  config: string;
  nextStepIdSuccess: string | null;
  nextStepIdFailure: string | null;
  sortOrder: number;
}

export interface WorkflowDefinitionDto {
  id: string;
  code: string;
  name: string;
  description: string | null;
  version: number;
  isActive: boolean;
  rootStepId: string | null;
  steps: WorkflowStepDto[];
}

export interface WorkflowDefinitionSummaryDto {
  id: string;
  code: string;
  name: string;
  version: number;
  isActive: boolean;
  stepCount: number;
  requestCount: number;
}

export interface WorkflowAuditTrailDto {
  id: string;
  stepId: string | null;
  stepName: string | null;
  toStepId: string | null;
  action: string;
  result: string | null;
  actorId: string | null;
  comment: string | null;
  occurredAt: string;
}

export interface WorkflowRequestDto {
  id: string;
  requestNumber: string;
  definitionId: string;
  definitionName: string;
  requesterId: string;
  currentStepId: string | null;
  currentStepName: string | null;
  status: WorkflowRequestStatus;
  payload: string;
  startedAt: string;
  completedAt: string | null;
  auditTrail: WorkflowAuditTrailDto[];
}

interface Paginated<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export interface WorkflowStepInput {
  id: string;
  type: WorkflowStepType;
  name: string;
  config: string;
  nextStepIdSuccess: string | null;
  nextStepIdFailure: string | null;
  sortOrder: number;
}

const DEFS = "/api/workflow-definitions";
const REQS = "/api/workflow-requests";

// ── Definitions ──
export const listWorkflowDefinitions = (isActive?: boolean) =>
  apiFetch<WorkflowDefinitionSummaryDto[]>(
    `${DEFS}${isActive === undefined ? "" : `?isActive=${isActive}`}`
  );

export const getWorkflowDefinition = (id: string) =>
  apiFetch<WorkflowDefinitionDto>(`${DEFS}/${id}`);

export const createWorkflowDefinition = (body: { code: string; name: string; description?: string }) =>
  apiFetch<WorkflowDefinitionDto>(DEFS, { method: "POST", body });

export const updateWorkflowDefinition = (
  id: string,
  body: { name: string; description?: string | null; isActive: boolean; rootStepId: string | null; steps: WorkflowStepInput[] }
) => apiFetch<WorkflowDefinitionDto>(`${DEFS}/${id}`, { method: "PUT", body });

export const deleteWorkflowDefinition = (id: string) =>
  apiFetch<unknown>(`${DEFS}/${id}`, { method: "DELETE" });

// ── Requests ──
export const listWorkflowRequests = (params: { status?: WorkflowRequestStatus; definitionId?: string } = {}) => {
  const q = new URLSearchParams({ pageNumber: "1", pageSize: "100" });
  if (params.status !== undefined) q.set("status", String(params.status));
  if (params.definitionId) q.set("definitionId", params.definitionId);
  return apiFetch<Paginated<WorkflowRequestDto>>(`${REQS}?${q.toString()}`).then((r) => r?.items ?? []);
};

export const getWorkflowRequest = (id: string) =>
  apiFetch<WorkflowRequestDto>(`${REQS}/${id}`);

export const getPendingWorkflowRequests = () =>
  apiFetch<WorkflowRequestDto[]>(`${REQS}/pending`);

export const startWorkflowRequest = (body: { definitionId: string; payload: string }) =>
  apiFetch<WorkflowRequestDto>(REQS, { method: "POST", body });

export const executeWorkflowStep = (id: string, body: { approved: boolean; comment?: string }) =>
  apiFetch<WorkflowRequestDto>(`${REQS}/${id}/execute`, { method: "POST", body });

export const cancelWorkflowRequest = (id: string, comment?: string) =>
  apiFetch<WorkflowRequestDto>(`${REQS}/${id}/cancel`, { method: "POST", body: { comment } });

// ── Display helpers ──
export const STEP_TYPE_LABEL: Record<WorkflowStepType, string> = {
  [WorkflowStepType.Approval]: "موافقة",
  [WorkflowStepType.Action]: "إجراء",
  [WorkflowStepType.Condition]: "شرط",
  [WorkflowStepType.End]: "نهاية",
};

export const STATUS_LABEL: Record<WorkflowRequestStatus, string> = {
  [WorkflowRequestStatus.Pending]: "قيد الانتظار",
  [WorkflowRequestStatus.InProgress]: "قيد التنفيذ",
  [WorkflowRequestStatus.Completed]: "مكتمل",
  [WorkflowRequestStatus.Cancelled]: "ملغي",
  [WorkflowRequestStatus.Rejected]: "مرفوض",
};

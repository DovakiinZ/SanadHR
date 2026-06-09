import { apiFetch } from "../api-client";

// ── Workflows engine (api/platform/workflows) ──
// Approval workflow definitions linked to Request Types and started on submission.

export interface WorkflowDefinition {
  id: string;
  code: string;
  nameEn: string;
  nameAr: string;
  triggerEntityType: string;
  isActive: boolean;
}

interface Paginated<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

const BASE = "/api/platform/workflows";

export function workflowLabel(w: WorkflowDefinition): string {
  return w.nameAr || w.nameEn || w.code;
}

export async function getWorkflowDefinitions(): Promise<WorkflowDefinition[]> {
  const q = new URLSearchParams({ pageNumber: "1", pageSize: "200" });
  const res = await apiFetch<Paginated<WorkflowDefinition>>(`${BASE}?${q.toString()}`);
  return res?.items ?? [];
}

export interface WorkflowInstance {
  id: string;
  workflowDefinitionId: string;
  entityType: string;
  entityId: string;
  status: string;
  startedAt: string;
}

// Start an approval workflow for a submitted request (entity = the form submission).
export async function startWorkflow(
  definitionCode: string,
  entityType: string,
  entityId: string
): Promise<WorkflowInstance> {
  return apiFetch<WorkflowInstance>(`${BASE}/start`, {
    method: "POST",
    body: { definitionCode, entityType, entityId },
  });
}

import { apiFetch } from "../api-client";

export interface AuditEntry {
  id: string;
  userId?: string | null;
  userEmail?: string | null;
  action: string;
  module?: string | null;
  entityType: string;
  entityId: string;
  oldValues?: string | null;
  newValues?: string | null;
  ipAddress?: string | null;
  timestamp: string;
}

interface Paginated<T> { items: T[]; pageNumber: number; pageSize: number; totalCount: number; }

export async function getAuditEntries(params: {
  entityType?: string; entityId?: string; action?: string; pageNumber?: number; pageSize?: number;
}): Promise<AuditEntry[]> {
  const q = new URLSearchParams();
  q.set("pageNumber", String(params.pageNumber ?? 1));
  q.set("pageSize", String(params.pageSize ?? 50));
  if (params.entityType) q.set("entityType", params.entityType);
  if (params.entityId) q.set("entityId", params.entityId);
  if (params.action) q.set("action", params.action);
  const res = await apiFetch<Paginated<AuditEntry>>(`/api/platform/audit?${q.toString()}`);
  return res?.items ?? [];
}

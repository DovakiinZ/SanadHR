import { apiFetch } from "../api-client";

export interface RestoreStep {
  order: number;
  role: string;
  status: string;
  decidedAt?: string | null;
  comment?: string | null;
}

export interface RestoreDto {
  id: string;
  employeeId: string;
  employeeName: string;
  employeeNumber: string;
  status: "PendingApproval" | "Approved" | "Rejected";
  currentStep: number;
  reason?: string | null;
  requestedAt: string;
  approvedAt?: string | null;
  rejectionReason?: string | null;
  steps: RestoreStep[];
}

export const requestRestore = (employeeId: string, reason?: string | null) =>
  apiFetch<RestoreDto>("/api/restores/request", { method: "POST", body: { employeeId, reason } });
export const getPendingRestores = () => apiFetch<RestoreDto[]>("/api/restores/pending");
export const getRestore = (id: string) => apiFetch<RestoreDto>(`/api/restores/${id}`);
export const decideRestore = (id: string, approve: boolean, comment?: string) =>
  apiFetch<RestoreDto>(`/api/restores/${id}/decide`, { method: "POST", body: { approve, comment } });

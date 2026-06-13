// Approval Center API.
import { apiFetch } from "../api-client";

export interface ApprovalTask {
  id: string;
  requestInstanceId: string;
  requestNumber: string;
  requestTypeNameAr: string;
  requestTypeNameEn: string;
  ownerName: string;
  department?: string | null;
  submittedAt: string;
  dueAt?: string | null;
  overdue: boolean;
  status: string; // Pending|Approved|Rejected|Returned|Skipped
  currentStepNameAr: string;
  leaveTypeName?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  daysCount?: number | null;
}

export interface Kv { label: string; value: string }
export interface ApprovalStep { stepOrder: number; stepNameAr: string; status: string; comment?: string | null; decidedAt?: string | null }
export interface ApprovalHistory { toStatus: string; noteAr?: string | null; at: string }

export interface ApprovalDetail {
  approvalId: string;
  requestInstanceId: string;
  requestNumber: string;
  requestTypeNameAr: string;
  requestTypeNameEn: string;
  status: string;
  ownerName: string;
  ownerNumber?: string | null;
  department?: string | null;
  jobTitle?: string | null;
  submittedAt: string;
  leaveTypeName?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  daysCount?: number | null;
  canAct: boolean;
  impact: string[];
  details: Kv[];
  attachments: Kv[];
  approvals: ApprovalStep[];
  timeline: ApprovalHistory[];
}

export const getMyApprovals = (all = false) =>
  apiFetch<ApprovalTask[]>(`/api/approvals/my${all ? "?all=true" : ""}`);

export const getApproval = (id: string) =>
  apiFetch<ApprovalDetail>(`/api/approvals/${id}`);

export const approveTask = (id: string, comment?: string) =>
  apiFetch<unknown>(`/api/approvals/${id}/approve`, { method: "POST", body: { comment } });

export const rejectTask = (id: string, comment?: string) =>
  apiFetch<unknown>(`/api/approvals/${id}/reject`, { method: "POST", body: { comment } });

export const returnTask = (id: string, comment?: string) =>
  apiFetch<unknown>(`/api/approvals/${id}/return`, { method: "POST", body: { comment } });

// ── tab classification ──
export type ApprovalTab = "pending" | "overdue" | "approved" | "rejected" | "returned";

export function classifyTab(t: ApprovalTask, tab: ApprovalTab): boolean {
  switch (tab) {
    case "pending": return t.status === "Pending";
    case "overdue": return t.status === "Pending" && t.overdue;
    case "approved": return t.status === "Approved";
    case "rejected": return t.status === "Rejected";
    case "returned": return t.status === "Returned";
  }
}

export function approvalStatusColor(status: string): string {
  switch (status) {
    case "Approved": return "bg-green-500/10 text-green-400 border-green-500/30";
    case "Rejected": return "bg-red-500/10 text-red-400 border-red-500/30";
    case "Returned": return "bg-orange-500/10 text-orange-400 border-orange-500/30";
    default: return "bg-amber-500/10 text-amber-400 border-amber-500/30";
  }
}

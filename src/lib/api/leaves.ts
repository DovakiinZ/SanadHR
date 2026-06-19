import { apiFetch, API_BASE_URL } from "../api-client";
import { getAccessToken } from "../auth-storage";

// ── Types (mirror HR.Modules.Platform.DTOs.Leaves) ─────────────────────────

export interface LeaveRecordRow {
  id: string;
  recordNumber: string;
  employeeId: string;
  employeeName?: string | null;
  employeeNumber?: string | null;
  departmentName?: string | null;
  branchName?: string | null;
  jobTitleName?: string | null;
  leaveTypeId: string;
  leaveTypeName?: string | null;
  startDate: string;
  endDate: string;
  daysCount: number;
  affectsBalance: boolean;
  balanceBefore: number;
  balanceAfter: number;
  status: string;
  source: string;
  requestInstanceId?: string | null;
  requestNumber?: string | null;
  approvedAt?: string | null;
  approvedByName?: string | null;
  notes?: string | null;
  hasAttachment: boolean;
  canceledAt?: string | null;
}

export interface LeaveTimeline {
  action: string;
  description?: string | null;
  at: string;
}

export interface LeaveDetail {
  record: LeaveRecordRow;
  nationality?: string | null;
  nationalId?: string | null;
  daysDeducted: number;
  attendanceDays: string[];
  affectsPayroll: boolean;
  attachmentUrl?: string | null;
  audit: LeaveTimeline[];
  timeline: LeaveTimeline[];
}

export interface LeaveBalance {
  leaveTypeId: string;
  leaveTypeName: string;
  affectsBalance: boolean;
  entitled: number;
  used: number;
  remaining: number;
}

export interface LeaveFilters {
  from?: string;
  to?: string;
  employeeId?: string;
  departmentId?: string;
  branchId?: string;
  leaveTypeId?: string;
  status?: string;
  source?: string;
}

export interface AssignLeaveInput {
  leaveTypeId: string;
  scope: "Employees" | "Department" | "Branch" | "JobTitle";
  employeeIds: string[];
  departmentId?: string | null;
  branchId?: string | null;
  jobTitleId?: string | null;
  startDate: string;
  endDate: string;
  notes?: string;
  attachmentUrl?: string | null;
}

export interface EditLeaveInput {
  leaveTypeId?: string;
  startDate?: string;
  endDate?: string;
  notes?: string;
  attachmentUrl?: string | null;
}

function qs(f: LeaveFilters): string {
  const q = new URLSearchParams();
  Object.entries(f).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== "") q.set(k, String(v)); });
  const s = q.toString();
  return s ? `?${s}` : "";
}

export async function listLeaves(filters: LeaveFilters = {}): Promise<LeaveRecordRow[]> {
  return (await apiFetch<LeaveRecordRow[]>(`/api/leaves${qs(filters)}`)) ?? [];
}

export function getLeaveDetail(id: string): Promise<LeaveDetail> {
  return apiFetch<LeaveDetail>(`/api/leaves/${id}`);
}

export function assignLeave(body: AssignLeaveInput): Promise<number> {
  return apiFetch<number>("/api/leaves/assign", { method: "POST", body });
}

export function editLeave(id: string, body: EditLeaveInput): Promise<unknown> {
  return apiFetch<unknown>(`/api/leaves/${id}`, { method: "PUT", body });
}

export function cancelLeave(id: string, reason?: string): Promise<unknown> {
  return apiFetch<unknown>(`/api/leaves/${id}/cancel`, { method: "POST", body: { reason } });
}

export function getEmployeeLeaveBalance(employeeId: string): Promise<LeaveBalance[]> {
  return apiFetch<LeaveBalance[]>(`/api/employees/${employeeId}/leave-balance`);
}

/** Generate + open the official Leave Record PDF in a new tab (and trigger print). */
export async function printLeaveRecord(id: string, print = true): Promise<void> {
  const token = getAccessToken();
  const res = await fetch(`${API_BASE_URL}/api/leaves/${id}/print`, {
    method: "POST",
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  if (!res.ok) throw new Error("تعذر إنشاء سجل الإجازة");
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const win = window.open(url, "_blank");
  if (win && print) win.addEventListener("load", () => win.print());
  setTimeout(() => URL.revokeObjectURL(url), 60000);
}

// ── Display helpers ─────────────────────────────────────────────────────────

export const LEAVE_STATUS_AR: Record<string, string> = {
  Approved: "معتمدة",
  Assigned: "مُعيّنة",
  Canceled: "ملغاة",
  Edited: "معدّلة",
};

export const LEAVE_STATUS_STYLE: Record<string, string> = {
  Approved: "bg-green-500/10 text-green-600",
  Assigned: "bg-blue-500/10 text-blue-600",
  Canceled: "bg-red-500/10 text-red-600",
  Edited: "bg-amber-500/10 text-amber-600",
};

export const LEAVE_SOURCE_AR: Record<string, string> = {
  Request: "طلب موظف",
  HRAssignment: "تعيين موارد بشرية",
  Import: "استيراد",
  System: "النظام",
};

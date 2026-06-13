// Request Center API — talks to the new object-driven request engine.
// Only fully-provisioned request types are returned, so every visible request is usable.
import { apiFetch, API_BASE_URL } from "../api-client";
import { getAccessToken } from "../auth-storage";

export interface RequestType {
  id: string;
  code: string;
  nameAr: string;
  nameEn: string;
  descriptionAr?: string | null;
  descriptionEn?: string | null;
  kind: string; // "System" | "Dynamic"
  categoryId?: string | null;
  icon?: string | null;
  color?: string | null;
  isSystem: boolean;
  hasWorkflow: boolean;
  generatesDocument: boolean;
}

export interface RequestField {
  id: string;
  code: string;
  nameAr: string;
  nameEn: string;
  fieldType: string; // Text/Number/Decimal/Currency/Date/DateTime/Boolean/Dropdown/TextArea/File...
  isRequired: boolean;
  placeholder?: string | null;
  options?: string | null; // JSON array of options
  sortOrder: number;
}

export interface RequestTypeDetail extends RequestType {
  formDefinitionId: string;
  isLeaveRequest: boolean;
  fields: RequestField[];
}

export interface LeaveRules {
  paid: boolean;
  paidPercentage: number;
  maxDays: number;
  annualBalance: number;
  requiresAttachment: boolean;
  affectsPayroll: boolean;
  affectsAttendance: boolean;
  countWeekends: boolean;
  countHolidays: boolean;
}

export interface LeaveTypeInfo {
  id: string;
  code: string;
  nameAr: string;
  nameEn: string;
  rules: LeaveRules;
  entitledDays: number;
  usedDays: number;
  remainingDays: number;
}

export interface LeavePreview {
  leaveTypeId: string;
  days: number;
  balanceBefore: number;
  balanceAfter: number;
  requiresAttachment: boolean;
  affectsPayroll: boolean;
  affectsAttendance: boolean;
  paidPercentage: number;
  paid: boolean;
  nextApproverAr?: string | null;
  nextApproverEn?: string | null;
  isValid: boolean;
  errors: string[];
}

export interface RequestApprovalStep {
  stepOrder: number;
  stepNameAr: string;
  stepNameEn: string;
  status: string;
  comment?: string | null;
  decidedAt?: string | null;
}

export interface RequestHistoryEntry {
  toStatus: string;
  noteAr?: string | null;
  noteEn?: string | null;
  at: string;
}

export interface RequestInstance {
  id: string;
  requestNumber: string;
  requestTypeId: string;
  requestTypeNameAr: string;
  requestTypeNameEn: string;
  status: string; // Submitted/InProgress/Approved/Rejected/Cancelled
  submittedAt: string;
  decidedAt?: string | null;
  currentStepOrder: number;
  startDate?: string | null;
  endDate?: string | null;
  daysCount?: number | null;
  generatedDocumentId?: string | null;
  approvals: RequestApprovalStep[];
  history: RequestHistoryEntry[];
}

export interface RequestValue {
  fieldCode: string;
  value?: string | null;
  fileUrl?: string | null;
  formFieldId?: string | null;
}

export const getRequestTypes = () =>
  apiFetch<RequestType[]>("/api/requests/types");

export const getRequestType = (id: string) =>
  apiFetch<RequestTypeDetail>(`/api/requests/types/${id}`);

export const submitRequest = (requestTypeId: string, values: RequestValue[]) =>
  apiFetch<RequestInstance>("/api/requests", { method: "POST", body: { requestTypeId, values } });

export const getMyRequests = () =>
  apiFetch<RequestInstance[]>("/api/requests/mine");

export const getInbox = () =>
  apiFetch<RequestInstance[]>("/api/requests/inbox");

export const getRequest = (id: string) =>
  apiFetch<RequestInstance>(`/api/requests/${id}`);

export const approveRequest = (id: string, comment?: string) =>
  apiFetch<RequestInstance>(`/api/requests/${id}/approve`, { method: "POST", body: { comment } });

export const rejectRequest = (id: string, comment?: string) =>
  apiFetch<RequestInstance>(`/api/requests/${id}/reject`, { method: "POST", body: { comment } });

export const cancelRequest = (id: string) =>
  apiFetch<RequestInstance>(`/api/requests/${id}/cancel`, { method: "POST" });

export const seedSystemRequests = () =>
  apiFetch<number>("/api/requests/seed-system", { method: "POST" });

// ── Leave (generic sub-typed request) ──
export const getLeaveTypes = () =>
  apiFetch<LeaveTypeInfo[]>("/api/requests/leave-types");

export const previewLeave = (leaveTypeId: string, startDate?: string, endDate?: string, hasAttachment = false) =>
  apiFetch<LeavePreview>("/api/requests/leave/preview", {
    method: "POST",
    body: { leaveTypeId, startDate, endDate, hasAttachment },
  });

// ── Admin: leave balances ──
export const getEmployeeLeaveBalances = (employeeId: string) =>
  apiFetch<LeaveTypeInfo[]>(`/api/requests/admin/leave-balances?employeeId=${employeeId}`);

export const setLeaveBalance = (employeeId: string, leaveTypeId: string, entitledDays: number, carriedForwardDays: number) =>
  apiFetch<unknown>("/api/requests/admin/leave-balances", {
    method: "PUT",
    body: { employeeId, leaveTypeId, entitledDays, carriedForwardDays },
  });

// ── Document download (official PDF, streamed with auth) ──
export async function downloadRequestDocument(requestId: string, fileName = "document.pdf"): Promise<void> {
  const token = getAccessToken();
  const res = await fetch(`${API_BASE_URL}/api/requests/${requestId}/document`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  if (!res.ok) throw new Error("تعذر تحميل المستند");
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  a.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

// ── helpers ──
export function requestStatusLabel(status: string): string {
  const map: Record<string, string> = {
    Submitted: "مُقدّم", InProgress: "قيد المعالجة", Approved: "مقبول",
    Rejected: "مرفوض", Cancelled: "ملغي", Draft: "مسودة", Pending: "بانتظار",
  };
  return map[status] ?? status;
}

export function requestStatusColor(status: string): string {
  switch (status) {
    case "Approved": return "bg-green-500/10 text-green-400 border-green-500/30";
    case "Rejected": return "bg-red-500/10 text-red-400 border-red-500/30";
    case "Cancelled": return "bg-zinc-500/10 text-zinc-400 border-zinc-500/30";
    case "InProgress": return "bg-amber-500/10 text-amber-400 border-amber-500/30";
    default: return "bg-blue-500/10 text-blue-400 border-blue-500/30";
  }
}

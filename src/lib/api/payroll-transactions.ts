import { apiFetch } from "../api-client";

export interface AttendanceDeductionSyncReport {
  created: number;
  updated: number;
  removed: number;
  skippedPosted: number;
  totalProcessed: number;
}

export interface AttendanceBreakdownRow {
  attendanceRecordId: string;
  date: string;
  penaltyKind: string;
  minutes: number;
  days: number;
  amountContribution: number;
}

export async function syncAttendanceDeductions(body: {
  definitionId: string;
  year: number;
  month: number;
  employeeIds?: string[];
}): Promise<AttendanceDeductionSyncReport> {
  return apiFetch<AttendanceDeductionSyncReport>("/api/payroll/attendance-deductions/sync", { method: "POST", body });
}

export async function getAttendanceBreakdown(transactionId: string): Promise<AttendanceBreakdownRow[]> {
  return apiFetch<AttendanceBreakdownRow[]>(`/api/payroll/transactions/${transactionId}/attendance-breakdown`);
}

// Mirror of backend PayrollTransactionKind / PayrollTransactionStatus (int enums serialized as numbers).
export type TransactionKind = 1 | 2; // 1 = Addition, 2 = Deduction
export type TransactionStatus = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7;
// 0 Draft · 1 PendingApproval · 2 Approved · 3 Rejected · 4 Cancelled · 5 CarriedForward · 6 Posted · 7 Reversed

export interface PayrollTransaction {
  id: string;
  kind: TransactionKind;
  employeeId: string;
  employeeName: string;
  employeeNumber: string;
  typeId: string;
  typeName: string;
  amount: number;
  transactionDate: string;
  effectiveDate: string;
  targetPeriodYear: number | null;
  targetPeriodMonth: number | null;
  isRecurring: boolean;
  recurrenceEndDate: string | null;
  notes: string | null;
  attachmentFileId: string | null;
  sourceModule: string;
  referenceType: string | null;
  referenceId: string | null;
  status: TransactionStatus;
  statusReason: string | null;
  payrollRunId: string | null;
  postedAt: string | null;
  reversesTransactionId: string | null;
  createdAt: string;
}

export interface CreateTransactionInput {
  kind: TransactionKind;
  employeeId: string;
  typeId: string;
  amount: number;
  effectiveDate: string;          // ISO date
  transactionDate?: string | null;
  isRecurring: boolean;
  recurrenceEndDate?: string | null;
  notes?: string | null;
  attachmentFileId?: string | null;
  submitImmediately: boolean;
}

export type UpdateTransactionInput = Omit<CreateTransactionInput, "kind" | "employeeId" | "submitImmediately">;

export interface TransactionFilter {
  kind?: TransactionKind;
  employeeId?: string;
  periodYear?: number;
  periodMonth?: number;
  typeId?: string;
  status?: TransactionStatus;
  dateFrom?: string;
  dateTo?: string;
}

const BASE = "/api/payroll/transactions";

export async function listTransactions(filter: TransactionFilter = {}): Promise<PayrollTransaction[]> {
  const q = new URLSearchParams();
  if (filter.kind != null) q.set("kind", String(filter.kind));
  if (filter.employeeId) q.set("employeeId", filter.employeeId);
  if (filter.periodYear != null) q.set("periodYear", String(filter.periodYear));
  if (filter.periodMonth != null) q.set("periodMonth", String(filter.periodMonth));
  if (filter.typeId) q.set("typeId", filter.typeId);
  if (filter.status != null) q.set("status", String(filter.status));
  if (filter.dateFrom) q.set("dateFrom", filter.dateFrom);
  if (filter.dateTo) q.set("dateTo", filter.dateTo);
  const qs = q.toString();
  return (await apiFetch<PayrollTransaction[]>(`${BASE}${qs ? `?${qs}` : ""}`)) ?? [];
}

export function getTransaction(id: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}`);
}

export function createTransaction(input: CreateTransactionInput): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(BASE, { method: "POST", body: input });
}

export function updateTransaction(id: string, input: UpdateTransactionInput): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}`, { method: "PUT", body: input });
}

export function submitTransaction(id: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}/submit`, { method: "POST" });
}

export function approveTransaction(id: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}/approve`, { method: "POST" });
}

export function rejectTransaction(id: string, reason: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}/reject`, { method: "POST", body: { reason } });
}

export function cancelTransaction(id: string, reason?: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}/cancel`, { method: "POST", body: { reason } });
}

export function setTransactionAttachment(id: string, fileId: string): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}/attachment`, { method: "POST", body: { fileId } });
}

export function deleteTransaction(id: string): Promise<void> {
  return apiFetch<void>(`${BASE}/${id}`, { method: "DELETE" });
}

export function reverseTransaction(
  id: string,
  body: { reason: string; createCorrection: boolean; correctedAmount?: number },
): Promise<PayrollTransaction> {
  return apiFetch<PayrollTransaction>(`${BASE}/${id}/reverse`, { method: "POST", body });
}

export interface TransactionImpactPreview {
  periodYear: number;
  periodMonth: number;
  cutoffDay: number;
  carriedAfterCutoff: boolean;
}

export function getTransactionImpact(effectiveDate: string): Promise<TransactionImpactPreview> {
  return apiFetch<TransactionImpactPreview>(
    `${BASE}/impact-preview?effectiveDate=${encodeURIComponent(effectiveDate)}`,
  );
}

export const TRANSACTION_STATUS_AR: Record<TransactionStatus, string> = {
  0: "مسودة", 1: "بانتظار الاعتماد", 2: "معتمد", 3: "مرفوض",
  4: "ملغى", 5: "مُرحّل", 6: "مُرحّل للسجل", 7: "معكوس",
};

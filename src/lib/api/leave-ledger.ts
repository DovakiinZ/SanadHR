import { apiFetch } from "../api-client";

// ── Types (mirror HR.Application.Engines.Leave) ────────────────────────────

export interface LeaveLedgerEntry {
  date: string;
  type: string; // Accrual | Usage | Adjustment | Forfeiture | Restoration
  amount: number; // signed delta (+ accrual / − usage)
  runningBalance: number;
  reason?: string | null;
  isUnpaidPeriod: boolean;
}

export interface LeaveLedgerGap {
  start: string;
  end: string;
  days: number;
}

export interface LeaveLedgerView {
  employeeId: string;
  leaveTypeId: string;
  currentBalance: number;
  accruedToDate: number;
  usedToDate: number;
  entries: LeaveLedgerEntry[];
  unpaidPeriods: LeaveLedgerGap[];
}

export function getLeaveLedger(employeeId: string, leaveTypeId?: string): Promise<LeaveLedgerView> {
  const q = leaveTypeId ? `?leaveTypeId=${leaveTypeId}` : "";
  return apiFetch<LeaveLedgerView>(`/api/employees/${employeeId}/leave-ledger${q}`);
}

export function recalculateLedger(employeeId: string, leaveTypeId?: string): Promise<LeaveLedgerView> {
  const q = leaveTypeId ? `?leaveTypeId=${leaveTypeId}` : "";
  return apiFetch<LeaveLedgerView>(`/api/employees/${employeeId}/leave-ledger/recalculate${q}`, { method: "POST" });
}

export const LEDGER_TYPE_AR: Record<string, string> = {
  Accrual: "استحقاق",
  Usage: "استخدام",
  Adjustment: "تسوية",
  Forfeiture: "إسقاط",
  Restoration: "استرجاع",
};

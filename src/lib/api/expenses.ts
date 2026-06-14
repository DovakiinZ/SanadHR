import { apiFetch } from "../api-client";

export interface ExpenseRecord {
  id: string;
  employeeId: string;
  employeeName?: string | null;
  category?: string | null;
  amount: number;
  currency: string;
  description?: string | null;
  receiptUrl?: string | null;
  status: string;
  decidedAt: string;
}

export const getExpenses = (scope: "mine" | "all" = "all") =>
  apiFetch<ExpenseRecord[]>(`/api/expenses${scope === "all" ? "?scope=all" : ""}`);

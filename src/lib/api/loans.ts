import { apiFetch } from "../api-client";

export interface LoanInstallment { dueMonth: string; amount: number; paid: boolean }

export interface LoanRecord {
  id: string;
  employeeId: string;
  employeeName?: string | null;
  loanType?: string | null;
  kind: string;        // Loan | Advance
  principal: number;
  installmentMonths: number;
  monthlyInstallment: number;
  status: string;
  startDate: string;
  installments: LoanInstallment[];
}

export const getLoans = (scope: "mine" | "all" = "all") =>
  apiFetch<LoanRecord[]>(`/api/loans${scope === "all" ? "?scope=all" : ""}`);

import { apiFetch, API_BASE_URL } from "../api-client";
import type { TerminationScenario, ContractTermType } from "./settlement";

export interface TerminationStep {
  order: number;
  role: string;
  status: string;
  decidedAt?: string | null;
  comment?: string | null;
}
export interface TerminationItem { labelAr: string; articleRef: string; amount: number }

export interface TerminationDto {
  id: string;
  employeeId: string;
  employeeName: string;
  employeeNumber: string;
  status: "Draft" | "PendingApproval" | "Approved" | "Rejected";
  currentStep: number;
  scenario: string;
  terminationDate: string;
  monthlyWage: number;
  serviceYears: number;
  gratuityAmount: number;
  article77Award: number;
  totalAward: number;
  currency: string;
  expenseId?: string | null;
  documentFileId?: string | null;
  approvedAt?: string | null;
  rejectionReason?: string | null;
  items: TerminationItem[];
  steps: TerminationStep[];
}

export interface TerminationRequestInput {
  employeeId: string;
  terminationDate: string;
  scenario: TerminationScenario;
  contractTermType: ContractTermType;
  contractEndDate?: string | null;
  notes?: string | null;
}

export const requestTermination = (body: TerminationRequestInput) =>
  apiFetch<TerminationDto>("/api/terminations/request", { method: "POST", body });
export const getPendingTerminations = () => apiFetch<TerminationDto[]>("/api/terminations/pending");
export const getTermination = (id: string) => apiFetch<TerminationDto>(`/api/terminations/${id}`);
export const decideTermination = (id: string, approve: boolean, comment?: string) =>
  apiFetch<TerminationDto>(`/api/terminations/${id}/decide`, { method: "POST", body: { approve, comment } });

export const fileUrl = (id: string) => `${API_BASE_URL}/api/files/${id}`;

export const SETTLEMENT_STATUS_AR: Record<string, string> = {
  Draft: "مسودة", PendingApproval: "بانتظار الاعتماد", Approved: "معتمد", Rejected: "مرفوض",
};
export const ROLE_AR: Record<string, string> = {
  Manager: "المدير المباشر", HR: "الموارد البشرية", Finance: "المالية",
};
export const STEP_STATUS_AR: Record<string, string> = {
  Pending: "بالانتظار", Approved: "تمت الموافقة", Rejected: "مرفوض", Skipped: "تم التخطي",
};

import { apiFetch } from "../api-client";

// ── Types (mirror HR.Modules.Employees DTOs + engine) ──────────────────────

export type TerminationScenario =
  | "NormalEmployerTermination"
  | "NormalResignation"
  | "Article77InvalidTermination"
  | "Article80ForCause"
  | "Article81EmployerBreachResignation";

export type ContractTermType = "Indefinite" | "FixedTerm";

export interface SettlementLine {
  labelEn: string;
  labelAr: string;
  articleRef: string;
  amount: number;
}

export interface SettlementResult {
  settlementId?: string | null;
  scenario: string;
  contractTermType: string;
  currency: string;
  monthlyWage: number;
  dailyWage: number;
  serviceYears: number;
  effectiveServiceDays: number;
  unpaidLeaveDays: number;
  gratuityAmount: number;
  article77Award: number;
  noticeCompensation: number;
  totalAward: number;
  lines: SettlementLine[];
}

export interface SettlementInput {
  terminationDate: string;
  scenario: TerminationScenario;
  contractTermType: ContractTermType;
  contractEndDate?: string | null;
  notes?: string | null;
}

export function previewSettlement(employeeId: string, input: SettlementInput): Promise<SettlementResult> {
  return apiFetch<SettlementResult>(`/api/employees/${employeeId}/settlement/preview`, { method: "POST", body: input });
}

export function terminateEmployee(employeeId: string, input: SettlementInput): Promise<SettlementResult> {
  return apiFetch<SettlementResult>(`/api/employees/${employeeId}/terminate`, { method: "POST", body: input });
}

// ── Display helpers ─────────────────────────────────────────────────────────

export const SCENARIO_AR: Record<string, string> = {
  NormalEmployerTermination: "إنهاء من صاحب العمل (سبب مشروع)",
  NormalResignation: "استقالة عادية",
  Article77InvalidTermination: "فصل غير مشروع (مادة 77)",
  Article80ForCause: "فصل لسبب مشروع بلا مكافأة (مادة 80)",
  Article81EmployerBreachResignation: "ترك العمل لإخلال صاحب العمل (مادة 81)",
};

/** Order shown in the scenario picker. */
export const SCENARIO_OPTIONS: TerminationScenario[] = [
  "NormalEmployerTermination",
  "NormalResignation",
  "Article77InvalidTermination",
  "Article80ForCause",
  "Article81EmployerBreachResignation",
];

export const CONTRACT_TERM_AR: Record<ContractTermType, string> = {
  Indefinite: "غير محدد المدة",
  FixedTerm: "محدد المدة",
};

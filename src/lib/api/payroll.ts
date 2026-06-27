import { apiFetch } from "../api-client";

export interface PayrollDefinitionDto {
  id: string;
  code: string;
  name: string;
  nameAr?: string | null;
  status: string;
  currentVersionId?: string | null;
  currency: string;
}

export interface PayrollRunListItem {
  id: string;
  runNumber: string;
  periodStart: string;
  periodEnd: string;
  state: string;
  currency: string;
  employeeCount: number;
  grossTotal: number;
  deductionTotal: number;
  netTotal: number;
  createdAt: string;
}

export interface PayslipDto {
  id: string;
  employeeId: string;
  employeeNumber: string;
  employeeName: string;
  currency: string;
  grossEarnings: number;
  totalDeductions: number;
  netAmount: number;
  ledgerPosted: boolean;
  componentsJson?: string | null;
}

export interface ValidationFindingDto {
  code: string;
  severity: string;
  message: string;
  employeeId?: string | null;
  employeeName?: string | null;
}

export interface RunTransitionDto { fromState: string; toState: string; at: string; reason?: string | null }

export interface PayrollRunDetail extends PayrollRunListItem {
  payrollDefinitionId: string;
  payrollDefinitionVersionId: string;
  ruleSetVersionId?: string | null;
  notes?: string | null;
  validatedAt?: string | null;
  approvedAt?: string | null;
  payslips: PayslipDto[];
  validation: ValidationFindingDto[];
  transitions: RunTransitionDto[];
}

export interface PayrollPreviewLineDto {
  employeeId: string; employeeNumber: string; employeeName: string;
  gross: number; deductions: number; net: number; hasErrors: boolean;
}
export interface PayrollPreviewDto {
  employeeCount: number; grossTotal: number; deductionTotal: number; netTotal: number; currency: string;
  isValid: boolean; findings: ValidationFindingDto[]; lines: PayrollPreviewLineDto[];
}

export const bootstrapPayroll = () => apiFetch<string>("/api/payroll/bootstrap", { method: "POST" });
export const getDefinitions = () => apiFetch<PayrollDefinitionDto[]>("/api/payroll/definitions");
export const previewPayroll = (definitionId: string, year: number, month: number) =>
  apiFetch<PayrollPreviewDto>("/api/payroll/preview", { method: "POST", body: { definitionId, year, month } });
export const listRuns = () => apiFetch<PayrollRunListItem[]>("/api/payroll/runs");
export const getRun = (id: string) => apiFetch<PayrollRunDetail>(`/api/payroll/runs/${id}`);
export const createRun = (definitionId: string, year: number, month: number) =>
  apiFetch<PayrollRunDetail>("/api/payroll/runs", { method: "POST", body: { definitionId, year, month } });
export const calculateRun = (id: string) => apiFetch<PayrollRunDetail>(`/api/payroll/runs/${id}/calculate`, { method: "POST" });
export const validateRun = (id: string) => apiFetch<PayrollRunDetail>(`/api/payroll/runs/${id}/validate`, { method: "POST" });
export const submitRun = (id: string) => apiFetch<PayrollRunDetail>(`/api/payroll/runs/${id}/submit`, { method: "POST" });
export const approveRun = (id: string) => apiFetch<PayrollRunDetail>(`/api/payroll/runs/${id}/approve`, { method: "POST" });
export const executeRun = (id: string) => apiFetch<PayrollRunDetail>(`/api/payroll/runs/${id}/execute`, { method: "POST" });
export const cancelRun = (id: string, reason: string) =>
  apiFetch<PayrollRunDetail>(`/api/payroll/runs/${id}/cancel`, { method: "POST", body: { reason } });

export const STATE_AR: Record<string, string> = {
  Draft: "مسودة", Preview: "معاينة", Validated: "تم التحقق", PendingApproval: "بانتظار الاعتماد",
  Approved: "معتمد", Executing: "قيد التنفيذ", Completed: "مكتمل", Locked: "مقفل", Archived: "مؤرشف",
  Failed: "فشل", Cancelled: "ملغي",
};

export function money(n: number, currency = "SAR"): string {
  return `${n.toLocaleString("ar-SA", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ${currency}`;
}

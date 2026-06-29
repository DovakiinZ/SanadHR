import { apiFetch } from "../api-client";

export type ScopeDimension = {
  key: string;
  nameEn: string;
  nameAr: string;
  valueSourceKind: "MasterData" | "StaticEnum" | "Custom";
  valueSourceRef: string | null;
  isAvailable: boolean;
  unavailableNote: string | null;
};

export type PayrollTypeListItem = {
  id: string;
  code: string;
  name: string;
  nameAr: string | null;
  categoryId: string | null;
  status: string;
  currentVersionId: string | null;
  versionCount: number;
};

export type PayrollVersion = {
  id: string;
  versionNumber: number;
  status: string;
  cutoffDay: number;
  dayBasis: string;
  closingDate: string | null;
  paymentDate: string | null;
  carryToNextPeriod: boolean;
  defaultExportFormatId: string | null;
  paymentMethodId: string | null;
  approvalWorkflowId: string | null;
  ruleSetVersionId: string | null;
  currency: string;
  frequency: string;
  effectiveFrom: string | null;
  effectiveTo: string | null;
  selectionScopeJson: string | null;
  calcSettingsJson: string | null;
  paymentMethodScopeJson: string | null;
};

export type PayrollTypeDetail = PayrollTypeListItem & { versions: PayrollVersion[] };

export const payrollTypesApi = {
  list: () => apiFetch<PayrollTypeListItem[]>("/api/payroll/types"),
  get: (id: string) => apiFetch<PayrollTypeDetail>(`/api/payroll/types/${id}`),
  create: (body: { code: string; name: string; nameAr?: string; categoryId?: string }) =>
    apiFetch<string>("/api/payroll/types", { method: "POST", body }),
  updateHeader: (
    id: string,
    body: { name: string; nameAr?: string; categoryId?: string; status: string }
  ) => apiFetch<boolean>(`/api/payroll/types/${id}`, { method: "PUT", body }),
  createVersion: (id: string) =>
    apiFetch<string>(`/api/payroll/types/${id}/versions`, { method: "POST", body: {} }),
  updateVersion: (id: string, vid: string, body: Partial<PayrollVersion>) =>
    apiFetch<boolean>(`/api/payroll/types/${id}/versions/${vid}`, { method: "PUT", body }),
  cloneVersion: (id: string, vid: string) =>
    apiFetch<string>(`/api/payroll/types/${id}/versions/${vid}/clone`, { method: "POST", body: {} }),
  publishVersion: (id: string, vid: string) =>
    apiFetch<boolean>(`/api/payroll/types/${id}/versions/${vid}/publish`, { method: "POST", body: {} }),
  simulate: (id: string, vid: string, year: number, month: number) =>
    apiFetch(`/api/payroll/types/${id}/versions/${vid}/simulate`, {
      method: "POST",
      body: { year, month },
    }),
  scopeDimensions: () => apiFetch<ScopeDimension[]>("/api/payroll/scope/dimensions"),
  resolveScope: (scopeJson: string) =>
    apiFetch<{ includedCount: number; excludedCount: number; warnings: string[] }>(
      "/api/payroll/scope/resolve",
      { method: "POST", body: { scopeJson } }
    ),
};

import { apiFetch } from "../api-client";

// ── Forms engine (api/platform/forms) ──
// Dynamic form definitions consumed by Request Types and rendered for end users.

export interface FormField {
  id: string;
  code: string;
  nameEn: string;
  nameAr: string;
  fieldType: string; // Text|Number|Decimal|Date|DateTime|Boolean|Dropdown|MultiSelect|TextArea|Email|Phone|Url|Currency|Percentage|File|Image
  isRequired: boolean;
  sortOrder: number;
  sectionName?: string | null;
  placeholder?: string | null;
  defaultValue?: string | null;
  validationRules?: string | null; // JSON
  options?: string | null;         // JSON (e.g. select options, or {lookup:"slug"})
}

export interface FormDefinition {
  id: string;
  code: string;
  nameEn: string;
  nameAr: string;
  description?: string | null;
  module: string;
  version: number;
  isPublished: boolean;
  isActive: boolean;
  fields: FormField[];
}

interface Paginated<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

const BASE = "/api/platform/forms";

export function formLabel(f: FormDefinition): string {
  return f.nameAr || f.nameEn || f.code;
}

// Lightweight list for dropdowns (pull a generous page to cover all tenant forms).
export async function getFormDefinitions(module?: string): Promise<FormDefinition[]> {
  const q = new URLSearchParams({ pageNumber: "1", pageSize: "200" });
  if (module) q.set("module", module);
  const res = await apiFetch<Paginated<FormDefinition>>(`${BASE}?${q.toString()}`);
  return res?.items ?? [];
}

export async function getFormDefinition(id: string): Promise<FormDefinition> {
  return apiFetch<FormDefinition>(`${BASE}/${id}`);
}

export interface FormSubmissionValueInput {
  formFieldId: string;
  fieldCode: string;
  value?: string | null;
  fileUrl?: string | null;
}

export interface FormSubmission {
  id: string;
  formDefinitionId: string;
  submittedById: string;
  submittedAt: string;
  status: string;
}

export async function submitForm(
  formDefinitionId: string,
  values: FormSubmissionValueInput[]
): Promise<FormSubmission> {
  return apiFetch<FormSubmission>(`${BASE}/${formDefinitionId}/submit`, {
    method: "POST",
    body: { formDefinitionId, values },
  });
}

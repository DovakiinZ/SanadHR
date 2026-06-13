// Document Template Builder API.
import { apiFetch } from "../api-client";

export interface DocumentTemplate {
  id: string;
  code: string;
  nameEn: string;
  nameAr: string;
  description?: string | null;
  module: string;
  status: string; // Draft | Published | Archived
  outputFormat: string;
  layoutJson?: string | null;
  bodyTemplate?: string | null;
  useBranding: boolean;
  pageTemplateId?: string | null;
  version: number;
  isSystem: boolean;
}

export interface TokenGroup {
  group: string;
  tokens: { token: string; label: string }[];
}

interface Paginated<T> { items: T[] }

export async function getDocumentTemplates(): Promise<DocumentTemplate[]> {
  const res = await apiFetch<Paginated<DocumentTemplate>>("/api/platform/documents/templates?pageNumber=1&pageSize=200");
  return res?.items ?? [];
}

export const getDocumentTemplate = (id: string) =>
  apiFetch<DocumentTemplate>(`/api/platform/documents/templates/${id}`);

export interface SaveTemplatePayload {
  nameAr: string; nameEn: string; description?: string;
  layoutJson?: string | null; bodyTemplate?: string | null; pageTemplateId?: string | null;
}

export const createDocumentTemplate = (p: SaveTemplatePayload & { code: string }) =>
  apiFetch<DocumentTemplate>("/api/platform/documents/templates", {
    method: "POST",
    body: { ...p, module: "Requests", outputFormat: 1, useBranding: true },
  });

export const updateDocumentTemplate = (id: string, p: SaveTemplatePayload) =>
  apiFetch<DocumentTemplate>(`/api/platform/documents/templates/${id}`, {
    method: "PUT",
    body: { id, ...p, module: "Requests", outputFormat: 1, useBranding: true },
  });

export const publishDocumentTemplate = (id: string) =>
  apiFetch<unknown>(`/api/platform/documents/templates/${id}/publish`, { method: "POST" });

export const deleteDocumentTemplate = (id: string) =>
  apiFetch<unknown>(`/api/platform/documents/templates/${id}`, { method: "DELETE" });

export const duplicateDocumentTemplate = (id: string) =>
  apiFetch<DocumentTemplate>(`/api/platform/documents/templates/${id}/duplicate`, { method: "POST" });

export const getTokenCatalog = () =>
  apiFetch<TokenGroup[]>("/api/platform/documents/token-catalog");

export const previewTemplateHtml = (body: string) =>
  apiFetch<string>("/api/platform/documents/preview-html", { method: "POST", body: { body } });

// ── Request-type ↔ template binding (legacy single print template) ──
export interface RequestTypeBinding {
  id: string;
  code: string;
  nameAr: string;
  nameEn: string;
  isActive: boolean;
  printTemplateId: string | null;
}

export const getRequestTypeBindings = () =>
  apiFetch<RequestTypeBinding[]>("/api/requests/types/admin");

export const setRequestTypePrintTemplate = (id: string, templateId: string | null) =>
  apiFetch<unknown>(`/api/requests/types/${id}/print-template`, { method: "PUT", body: { templateId } });

// ── Request → template mappings (multiple templates per type, each with a trigger) ──
export type TriggerEvent = "Submitted" | "FirstApproval" | "FinalApproval" | "Rejected" | "Completed";

export const TRIGGER_LABELS: Record<TriggerEvent, string> = {
  Submitted: "عند التقديم",
  FirstApproval: "أول موافقة",
  FinalApproval: "الموافقة النهائية",
  Rejected: "عند الرفض",
  Completed: "عند الاكتمال",
};

export interface RequestTemplateMapping {
  id: string;
  requestTypeId: string;
  documentTemplateId: string;
  templateNameAr?: string | null;
  triggerEvent: TriggerEvent;
  isSystem: boolean;
  isActive: boolean;
}

export const getRequestTemplateMappings = (typeId: string) =>
  apiFetch<RequestTemplateMapping[]>(`/api/requests/types/${typeId}/templates`);

export const addRequestTemplateMapping = (typeId: string, templateId: string, triggerEvent: TriggerEvent) =>
  apiFetch<RequestTemplateMapping>(`/api/requests/types/${typeId}/templates`, {
    method: "POST", body: { templateId, triggerEvent },
  });

export const deleteRequestTemplateMapping = (mappingId: string) =>
  apiFetch<unknown>(`/api/requests/template-mappings/${mappingId}`, { method: "DELETE" });

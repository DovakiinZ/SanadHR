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
  bodyTemplate: string;
  useBranding: boolean;
  version: number;
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

export const createDocumentTemplate = (p: {
  code: string; nameAr: string; nameEn: string; description?: string; bodyTemplate: string;
}) => apiFetch<DocumentTemplate>("/api/platform/documents/templates", {
  method: "POST",
  body: { ...p, module: "Requests", outputFormat: 1, useBranding: true },
});

export const updateDocumentTemplate = (id: string, p: {
  nameAr: string; nameEn: string; description?: string; bodyTemplate: string;
}) => apiFetch<DocumentTemplate>(`/api/platform/documents/templates/${id}`, {
  method: "PUT",
  body: { id, ...p, module: "Requests", outputFormat: 1, useBranding: true },
});

export const publishDocumentTemplate = (id: string) =>
  apiFetch<unknown>(`/api/platform/documents/templates/${id}/publish`, { method: "POST" });

export const deleteDocumentTemplate = (id: string) =>
  apiFetch<unknown>(`/api/platform/documents/templates/${id}`, { method: "DELETE" });

export const getTokenCatalog = () =>
  apiFetch<TokenGroup[]>("/api/platform/documents/token-catalog");

export const previewTemplateHtml = (body: string) =>
  apiFetch<string>("/api/platform/documents/preview-html", { method: "POST", body: { body } });

// ── Request-type ↔ template binding (entity-level; this is what actually drives PDF output) ──
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

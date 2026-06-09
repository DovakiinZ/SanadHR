import { apiFetch } from "../api-client";

// ── Documents engine (api/platform/documents) ──
// Document templates a Request Type can generate on completion.

export interface DocumentTemplate {
  id: string;
  code: string;
  nameEn: string;
  nameAr: string;
  description?: string | null;
  module: string;
  status: string;
  outputFormat: string;
  isActive: boolean;
}

interface Paginated<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

const BASE = "/api/platform/documents";

export function documentLabel(d: DocumentTemplate): string {
  return d.nameAr || d.nameEn || d.code;
}

export async function getDocumentTemplates(): Promise<DocumentTemplate[]> {
  const q = new URLSearchParams({ pageNumber: "1", pageSize: "200" });
  const res = await apiFetch<Paginated<DocumentTemplate>>(`${BASE}/templates?${q.toString()}`);
  return res?.items ?? [];
}

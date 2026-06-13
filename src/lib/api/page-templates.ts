// Page Templates API — reusable document chrome (header/footer/margins/watermark).
import { apiFetch } from "../api-client";

export interface PageTemplate {
  id: string;
  code: string;
  nameEn: string;
  nameAr: string;
  description?: string | null;
  headerConfig?: string | null; // JSON
  footerConfig?: string | null; // JSON
  margins?: string | null;      // JSON { top,right,bottom,left }
  watermark?: string | null;    // JSON { text }
  isSystem: boolean;
  isActive: boolean;
  sortOrder: number;
}

export interface HeaderConfig {
  showLogo?: boolean; logoPlacement?: "Left" | "Center" | "Right";
  showIdentity?: boolean; showCrVat?: boolean; showContact?: boolean; customText?: string;
}
export interface FooterConfig {
  showQr?: boolean; qrPlacement?: "Left" | "Center" | "Right";
  showStamp?: boolean; showSignatures?: boolean; showGeneratedDate?: boolean; customText?: string;
}
export interface Margins { top?: number; right?: number; bottom?: number; left?: number }
export interface Watermark { text?: string }

export const getPageTemplates = () =>
  apiFetch<PageTemplate[]>("/api/platform/page-templates");

export const getPageTemplate = (id: string) =>
  apiFetch<PageTemplate>(`/api/platform/page-templates/${id}`);

export interface SavePageTemplatePayload {
  nameAr: string; nameEn: string; description?: string;
  headerConfig?: string | null; footerConfig?: string | null; margins?: string | null; watermark?: string | null;
}

export const createPageTemplate = (p: SavePageTemplatePayload & { code: string }) =>
  apiFetch<PageTemplate>("/api/platform/page-templates", { method: "POST", body: p });

export const updatePageTemplate = (id: string, p: SavePageTemplatePayload) =>
  apiFetch<PageTemplate>(`/api/platform/page-templates/${id}`, { method: "PUT", body: { id, ...p } });

export const deletePageTemplate = (id: string) =>
  apiFetch<unknown>(`/api/platform/page-templates/${id}`, { method: "DELETE" });

export function parseJson<T>(s?: string | null): T {
  if (!s) return {} as T;
  try { return JSON.parse(s) as T; } catch { return {} as T; }
}

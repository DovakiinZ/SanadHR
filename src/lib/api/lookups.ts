import { apiFetch } from "../api-client";

// Read-only dropdown feed from the Master Data engine (api/lookups/{type-or-slug}).
export interface LookupItem {
  id: string;
  code: string;
  nameAr: string;
  nameEn: string;
  label: string;
  description?: string | null;
  color?: string | null;
  icon?: string | null;
  metadata?: Record<string, unknown> | null;
}

export async function getLookup(slug: string): Promise<LookupItem[]> {
  return (await apiFetch<LookupItem[]>(`/api/lookups/${slug}`)) ?? [];
}

export function lookupLabel(item: LookupItem): string {
  return item.nameAr || item.label || item.nameEn || item.code;
}

import { apiFetch } from "../api-client";

// Core organization entities (canonical, with manager/hierarchy) — dropdown sources.
export interface OrgOption {
  id: string;
  name: string;
  nameAr?: string | null;
}

export async function getDepartments(): Promise<OrgOption[]> {
  return (await apiFetch<OrgOption[]>("/api/departments")) ?? [];
}

export async function getBranches(): Promise<OrgOption[]> {
  return (await apiFetch<OrgOption[]>("/api/branches")) ?? [];
}

export function orgLabel(o: OrgOption): string {
  return o.nameAr || o.name;
}

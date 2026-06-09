import { apiFetch } from "../api-client";

// Core organization entities (canonical, with manager/hierarchy).

// ── Lightweight dropdown options (used by forms) ──
export interface OrgOption {
  id: string;
  name: string;
  nameAr?: string | null;
}

export function orgLabel(o: OrgOption): string {
  return o.nameAr || o.name;
}

export async function getDepartments(): Promise<OrgOption[]> {
  return (await apiFetch<OrgOption[]>("/api/departments")) ?? [];
}

export async function getBranches(): Promise<OrgOption[]> {
  return (await apiFetch<OrgOption[]>("/api/branches")) ?? [];
}

// ── Departments (full management) ──
export interface Department {
  id: string;
  name: string;
  nameAr?: string | null;
  code?: string | null;
  description?: string | null;
  parentDepartmentId?: string | null;
  parentDepartmentName?: string | null;
  managerId?: string | null;
  managerName?: string | null;
  deputyManagerId?: string | null;
  deputyManagerName?: string | null;
  branchId?: string | null;
  branchName?: string | null;
  costCenterId?: string | null;
  costCenterName?: string | null;
  isActive: boolean;
}

export interface DepartmentInput {
  name: string;
  nameAr?: string;
  code?: string;
  description?: string;
  parentDepartmentId?: string | null;
  managerId?: string | null;
  deputyManagerId?: string | null;
  branchId?: string | null;
  costCenterId?: string | null;
  isActive: boolean;
}

export async function listDepartments(): Promise<Department[]> {
  return (await apiFetch<Department[]>("/api/departments")) ?? [];
}

export async function createDepartment(input: DepartmentInput): Promise<Department> {
  return apiFetch<Department>("/api/departments", { method: "POST", body: input });
}

export async function updateDepartment(id: string, input: DepartmentInput): Promise<Department> {
  return apiFetch<Department>(`/api/departments/${id}`, { method: "PUT", body: input });
}

export async function deleteDepartment(id: string): Promise<void> {
  await apiFetch<unknown>(`/api/departments/${id}`, { method: "DELETE" });
}

// Move a department under a new parent (org-chart drag). Backend guards against cycles.
export async function reparentDepartment(id: string, parentDepartmentId: string | null): Promise<Department> {
  return apiFetch<Department>(`/api/departments/${id}/parent`, {
    method: "PUT",
    body: { parentDepartmentId },
  });
}

// ── Branches (full management) ──
export interface Branch {
  id: string;
  name: string;
  nameAr?: string | null;
  code?: string | null;
  city?: string | null;
  address?: string | null;
  phone?: string | null;
  isMainBranch: boolean;
  latitude?: number | null;
  longitude?: number | null;
  geofenceRadiusMeters?: number | null;
  isActive: boolean;
}

export interface BranchInput {
  name: string;
  nameAr?: string;
  code?: string;
  city?: string;
  address?: string;
  phone?: string;
  isMainBranch: boolean;
  latitude?: number | null;
  longitude?: number | null;
  geofenceRadiusMeters?: number | null;
  isActive: boolean;
}

export async function listBranches(): Promise<Branch[]> {
  return (await apiFetch<Branch[]>("/api/branches")) ?? [];
}

export async function createBranch(input: BranchInput): Promise<Branch> {
  return apiFetch<Branch>("/api/branches", { method: "POST", body: input });
}

export async function updateBranch(id: string, input: BranchInput): Promise<Branch> {
  return apiFetch<Branch>(`/api/branches/${id}`, { method: "PUT", body: input });
}

export async function deleteBranch(id: string): Promise<void> {
  await apiFetch<unknown>(`/api/branches/${id}`, { method: "DELETE" });
}

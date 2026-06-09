import { apiFetch } from "../api-client";

// ── Master Data engine (api/platform/master-data) ──
// Tenant-scoped catalogs (Job Titles, Nationalities, Contract Types, Leave Types, …).
// Each item is a MasterDataItem discriminated by ObjectType. Powers every governed dropdown.

export interface MasterDataType {
  objectType: string;
  slug: string;
  count: number;
}

export interface MasterDataItem {
  id: string;
  objectType: string;
  code: string;
  nameAr: string;
  nameEn: string;
  description?: string | null;
  color?: string | null;
  icon?: string | null;
  sortOrder: number;
  isSystemDefault: boolean;
  isActive: boolean;
  metadata?: Record<string, unknown> | null;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface MasterDataItemInput {
  code?: string;
  nameAr: string;
  nameEn: string;
  description?: string;
  color?: string;
  icon?: string;
  sortOrder?: number;
  isActive?: boolean;
  metadata?: Record<string, unknown>; // type-specific rules/behavior (sent as a JSON object)
}

// Merge an item's metadata object (Dictionary<string,object> from the API) onto a typed fallback.
export function parseMetadata<T>(item: { metadata?: Record<string, unknown> | null }, fallback: T): T {
  if (!item.metadata) return fallback;
  return { ...fallback, ...(item.metadata as Partial<T>) };
}

const BASE = "/api/platform/master-data";

// Department & Branch are canonical Core org entities (managed separately) — not pure lookups.
export const EXCLUDED_TYPES = new Set(["Department", "Branch"]);

export async function getMasterDataTypes(): Promise<MasterDataType[]> {
  const types = (await apiFetch<MasterDataType[]>(`${BASE}/types`)) ?? [];
  return types.filter((t) => !EXCLUDED_TYPES.has(t.objectType));
}

export async function getMasterDataItems(
  objectType: string,
  opts?: { search?: string; includeInactive?: boolean }
): Promise<MasterDataItem[]> {
  const q = new URLSearchParams();
  q.set("objectType", objectType);
  if (opts?.search) q.set("search", opts.search);
  if (opts?.includeInactive) q.set("includeInactive", "true");
  return (await apiFetch<MasterDataItem[]>(`${BASE}?${q.toString()}`)) ?? [];
}

export async function createMasterDataItem(
  objectType: string,
  input: MasterDataItemInput
): Promise<MasterDataItem> {
  return apiFetch<MasterDataItem>(BASE, {
    method: "POST",
    body: { objectType, sortOrder: 0, isActive: true, ...input },
  });
}

export async function updateMasterDataItem(
  id: string,
  input: MasterDataItemInput
): Promise<MasterDataItem> {
  return apiFetch<MasterDataItem>(`${BASE}/${id}`, {
    method: "PUT",
    body: { id, sortOrder: 0, isActive: true, ...input },
  });
}

export async function deactivateMasterDataItem(id: string): Promise<void> {
  await apiFetch<unknown>(`${BASE}/${id}/deactivate`, { method: "POST" });
}

export async function deleteMasterDataItem(id: string): Promise<void> {
  await apiFetch<unknown>(`${BASE}/${id}`, { method: "DELETE" });
}

// Arabic labels for the catalog types (Department/Branch excluded — Core entities).
export const TYPE_LABELS_AR: Record<string, string> = {
  JobTitle: "المسميات الوظيفية",
  Position: "المناصب",
  Grade: "الدرجات الوظيفية",
  CostCenter: "مراكز التكلفة",
  EmploymentType: "أنواع التوظيف",
  ContractType: "أنواع العقود",
  LeaveType: "أنواع الإجازات",
  AllowanceType: "أنواع البدلات",
  DeductionType: "أنواع الاستقطاعات",
  DocumentType: "أنواع المستندات",
  RequestType: "أنواع الطلبات",
  RequestCategory: "فئات الطلبات",
  ShiftType: "أنواع الورديات",
  AttendancePolicy: "سياسات الحضور",
  PayrollGroup: "مجموعات الرواتب",
  LeavePolicy: "سياسات الإجازات",
  WorkLocation: "مواقع العمل",
  ExpenseCategory: "فئات المصروفات",
  LoanType: "أنواع السلف",
  AssetType: "أنواع الأصول",
  RecruitmentSource: "مصادر التوظيف",
  CandidateStage: "مراحل المرشحين",
  Tag: "الوسوم",
  Skill: "المهارات",
  Bank: "البنوك",
  Nationality: "الجنسيات",
};

export function typeLabelAr(objectType: string): string {
  return TYPE_LABELS_AR[objectType] ?? objectType;
}

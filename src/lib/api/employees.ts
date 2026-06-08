import { apiFetch } from "../api-client";
import { Employee } from "@/types";

// Raw backend DTO (camelCase JSON from the .NET API).
export interface ApiEmployee {
  id: string;
  employeeNumber: string;
  firstName: string;
  firstNameAr?: string | null;
  lastName: string;
  lastNameAr?: string | null;
  fullName: string;
  fullNameAr?: string | null;
  email: string;
  phone?: string | null;
  gender: string;
  genderAr: string;
  dateOfBirth: string;
  nationalId?: string | null;
  nationality?: string | null;
  status: string;
  statusAr: string;
  contractType: string;
  contractTypeAr: string;
  hireDate: string;
  terminationDate?: string | null;
  jobTitle?: string | null;
  jobTitleAr?: string | null;
  departmentId?: string | null;
  departmentName?: string | null;
  branchId?: string | null;
  branchName?: string | null;
  managerId?: string | null;
  managerName?: string | null;
  basicSalary: number;
  currency?: string | null;
  photoUrl?: string | null;
  createdAt: string;
}

interface PaginatedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Form input collected by the (Arabic) Employee form.
export interface EmployeeInput {
  employeeNumber?: string;
  name: string;
  nationalId: string;
  phone: string;
  email: string;
  dateOfBirth: string;
  gender: string; // "ذكر" | "أنثى"
  nationality: string;
  position: string;
  joinDate: string;
  contractType: string; // "دوام كامل" | "دوام جزئي" | "عقد مؤقت"
  salary: number;
  status?: string; // edit only — backend statusAr string
}

// Enum maps — values MUST match the backend integer enum values.
const GENDER_TO_INT: Record<string, number> = { "ذكر": 1, "أنثى": 2 };
const CONTRACT_TO_INT: Record<string, number> = {
  "دوام كامل": 1,
  "دوام جزئي": 2,
  "عقد مؤقت": 3,
};
// Keyed on the backend's exact Arabic status strings (EmployeeMappingProfile).
const STATUS_TO_INT: Record<string, number> = {
  "نشط": 1,
  "في إجازة": 2,
  "موقوف": 3,
  "منتهي": 4,
  "مستقيل": 5,
};

// Postgres timestamptz columns require UTC — send full ISO with Z.
function toIsoUtc(dateStr: string): string {
  const d = new Date(dateStr);
  return isNaN(d.getTime()) ? new Date().toISOString() : d.toISOString();
}

function splitName(name: string): { first: string; last: string } {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  const first = parts[0] || name.trim();
  const last = parts.slice(1).join(" ") || parts[0] || name.trim();
  return { first, last };
}

// Map backend DTO -> existing Arabic display Employee (keeps the current UI intact).
export function toDisplayEmployee(a: ApiEmployee): Employee {
  return {
    id: a.id,
    employeeId: a.employeeNumber,
    name: a.fullNameAr || a.fullName,
    email: a.email,
    phone: a.phone ?? "",
    nationalId: a.nationalId ?? "",
    dateOfBirth: a.dateOfBirth ? a.dateOfBirth.slice(0, 10) : "",
    gender: (a.genderAr as Employee["gender"]) || "ذكر",
    nationality: a.nationality ?? "",
    department: a.departmentName ?? "—",
    position: a.jobTitleAr || a.jobTitle || "—",
    joinDate: a.hireDate ? a.hireDate.slice(0, 10) : "",
    contractType: (a.contractTypeAr as Employee["contractType"]) || "دوام كامل",
    salary: a.basicSalary,
    status: (a.statusAr as Employee["status"]) || "نشط",
    leaveBalance: 0,
    attendanceRate: 0,
  };
}

export async function getEmployees(params?: {
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}): Promise<Employee[]> {
  const q = new URLSearchParams();
  q.set("pageNumber", String(params?.pageNumber ?? 1));
  q.set("pageSize", String(params?.pageSize ?? 200));
  if (params?.search) q.set("search", params.search);
  const result = await apiFetch<PaginatedResult<ApiEmployee>>(`/api/employees?${q.toString()}`);
  return (result?.items ?? []).map(toDisplayEmployee);
}

export async function getEmployee(id: string): Promise<Employee> {
  const a = await apiFetch<ApiEmployee>(`/api/employees/${id}`);
  return toDisplayEmployee(a);
}

export async function createEmployee(input: EmployeeInput): Promise<ApiEmployee> {
  const { first, last } = splitName(input.name);
  const payload = {
    employeeNumber: input.employeeNumber?.trim() || `EMP-${Date.now().toString().slice(-6)}`,
    firstName: first,
    firstNameAr: first,
    lastName: last,
    lastNameAr: last,
    email: input.email,
    phone: input.phone,
    gender: GENDER_TO_INT[input.gender] ?? 1,
    dateOfBirth: toIsoUtc(input.dateOfBirth),
    nationalId: input.nationalId,
    nationality: input.nationality,
    contractType: CONTRACT_TO_INT[input.contractType] ?? 1,
    hireDate: toIsoUtc(input.joinDate),
    jobTitle: input.position,
    jobTitleAr: input.position,
    basicSalary: input.salary,
    currency: "SAR",
  };
  return apiFetch<ApiEmployee>("/api/employees", { method: "POST", body: payload });
}

export async function updateEmployee(id: string, input: EmployeeInput): Promise<ApiEmployee> {
  const { first, last } = splitName(input.name);
  const payload = {
    id,
    firstName: first,
    firstNameAr: first,
    lastName: last,
    lastNameAr: last,
    email: input.email,
    phone: input.phone,
    gender: GENDER_TO_INT[input.gender] ?? 1,
    dateOfBirth: toIsoUtc(input.dateOfBirth),
    nationalId: input.nationalId,
    nationality: input.nationality,
    status: STATUS_TO_INT[input.status ?? "نشط"] ?? 1,
    contractType: CONTRACT_TO_INT[input.contractType] ?? 1,
    jobTitle: input.position,
    jobTitleAr: input.position,
    basicSalary: input.salary,
  };
  return apiFetch<ApiEmployee>(`/api/employees/${id}`, { method: "PUT", body: payload });
}

export async function deleteEmployee(id: string): Promise<void> {
  await apiFetch<unknown>(`/api/employees/${id}`, { method: "DELETE" });
}
